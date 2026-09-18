using Azure.Messaging.ServiceBus;
using DentalClinic.Appointments.Projections.Worker;
using DentalClinic.Appointments.ReadModel.DependencyInjection;
using DentalClinic.Appointments.ReadModel.IntegrationEvents;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration["AzureServiceBus:ConnectionString"];
var topicName = builder.Configuration["AzureServiceBus:TopicName"];
var subscriptionName = builder.Configuration["AzureServiceBus:SubscriptionName"];
var readConnectionString = builder.Configuration["ConnectionStrings:ReadDatabase"];

if (string.IsNullOrWhiteSpace(connectionString) ||
    string.IsNullOrWhiteSpace(topicName) ||
    string.IsNullOrWhiteSpace(subscriptionName))
{
    throw new InvalidOperationException(
        "AzureServiceBus settings are not configured " +
        "(ConnectionString, TopicName, SubscriptionName).");
}

if (string.IsNullOrWhiteSpace(readConnectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:ReadDatabase is not configured.");
}

builder.Services.AddSingleton(new ServiceBusClient(connectionString));

builder.Services.AddSingleton(serviceProvider =>
{
    var client = serviceProvider.GetRequiredService<ServiceBusClient>();

    return client.CreateProcessor(
        topicName,
        subscriptionName,
        new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 1
        });
});

builder.Services.AddReadModel(readConnectionString);
builder.Services.AddScoped<IntegrationEventDispatcher>();
builder.Services.AddHostedService<AppointmentProjectionsWorker>();

var host = builder.Build();

await host.RunAsync();