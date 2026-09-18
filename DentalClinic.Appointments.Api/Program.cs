using Azure.Messaging.ServiceBus;
using DentalClinic.Appointments.Api.Endpoints;
using DentalClinic.Appointments.Application.Abstractions.Messaging;
using DentalClinic.Appointments.Application.Abstractions.Persistence;
using DentalClinic.Appointments.Application.Features.Appointments.Commands.CancelAppointment;
using DentalClinic.Appointments.Application.Features.Appointments.Commands.RescheduleAppointment;
using DentalClinic.Appointments.Application.Features.Appointments.Commands.ScheduleAppointment;
using DentalClinic.Appointments.Application.Features.Appointments.Queries.GetAppointmentById;
using DentalClinic.Appointments.Infrastructure.Messaging;
using DentalClinic.Appointments.Infrastructure.Persistence;
using DentalClinic.Appointments.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppointmentsDbContext>(options =>
    options.UseInMemoryDatabase("DentalClinicAppointments"));
builder.Services.AddScoped<IAppointmentWriteRepository, AppointmentWriteRepository>();
builder.Services.AddScoped<IAppointmentReadRepository, AppointmentReadRepository>();
builder.Services.AddScoped<IValidator<ScheduleAppointmentCommand>,
    ScheduleAppointmentCommandValidator>();
builder.Services.AddScoped<IValidator<RescheduleAppointmentCommand>,
    RescheduleAppointmentCommandValidator>();
builder.Services.AddScoped<ICommandHandler<ScheduleAppointmentCommand, Guid>,
    ScheduleAppointmentCommandHandler>();
builder.Services.AddScoped<ICommandHandler<RescheduleAppointmentCommand, Guid>,
    RescheduleAppointmentCommandHandler>();
builder.Services.AddScoped<
    IQueryHandler<GetAppointmentByIdQuery, AppointmentDetails?>,
    GetAppointmentByIdQueryHandler>();
builder.Services.AddScoped<IValidator<CancelAppointmentCommand>,
    CancelAppointmentCommandValidator>();
builder.Services.AddScoped<ICommandHandler<CancelAppointmentCommand, Guid>,
    CancelAppointmentCommandHandler>();
builder.Services.AddScoped<IOutbox, EfOutbox>();
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();

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