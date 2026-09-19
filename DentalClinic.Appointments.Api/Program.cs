using Azure.Messaging.ServiceBus;
using DentalClinic.Appointments.Api.Endpoints;
using DentalClinic.Appointments.Application.Abstractions.Behaviors;
using DentalClinic.Appointments.Application.Abstractions.Messaging;
using DentalClinic.Appointments.Application.Abstractions.Persistence;
using DentalClinic.Appointments.Application.Features.Appointments.Commands.ScheduleAppointment;
using DentalClinic.Appointments.Infrastructure.Messaging;
using DentalClinic.Appointments.Infrastructure.Persistence;
using DentalClinic.Appointments.Infrastructure.Repositories;
using DentalClinic.Appointments.ReadModel.DependencyInjection;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppointmentsDbContext>(options =>
    options.UseInMemoryDatabase("DentalClinicAppointments"));
builder.Services.AddScoped<IAppointmentWriteRepository, AppointmentWriteRepository>();
builder.Services.AddScoped<IOutbox, EfOutbox>();
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();

var readDataProvider =
    builder.Configuration["ReadDataProvider"] ?? "Postgres";

if (readDataProvider == "InMemory")
{
    builder.Services.AddReadModelInMemory();
}
else
{
    var readConnectionString =
        builder.Configuration["ConnectionStrings:ReadDatabase"];

    if (string.IsNullOrWhiteSpace(readConnectionString))
    {
        throw new InvalidOperationException(
            "ConnectionStrings:ReadDatabase no está configurado. El read model se " +
            "almacena en PostgreSQL: define la variable ConnectionStrings__ReadDatabase " +
            "o levanta la infraestructura con 'docker compose up -d --build'.");
    }

    builder.Services.AddReadModel(readConnectionString);
}

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(ScheduleAppointmentCommand).Assembly));

builder.Services.AddValidatorsFromAssembly(
    typeof(ScheduleAppointmentCommand).Assembly);

builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

var serviceBusConnectionString =
    builder.Configuration["AzureServiceBus:ConnectionString"];
var serviceBusTopicName =
    builder.Configuration["AzureServiceBus:TopicName"];

if (!string.IsNullOrWhiteSpace(serviceBusConnectionString) &&
    !string.IsNullOrWhiteSpace(serviceBusTopicName))
{
    builder.Services.AddSingleton(
        new ServiceBusClient(serviceBusConnectionString));
    builder.Services.AddSingleton(serviceProvider =>
        serviceProvider
            .GetRequiredService<ServiceBusClient>()
            .CreateSender(serviceBusTopicName));
    builder.Services.AddScoped<AzureServiceBusOutboxPublisher>();
    builder.Services.AddHostedService<OutboxProcessor>();
}

var app = builder.Build();

app.UseHttpsRedirection();

app.MapAppointmentEndpoints();

app.Run();

public partial class Program
{
}