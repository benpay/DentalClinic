using Azure.Messaging.ServiceBus;
using DentalClinic.Appointments.ReadModel.AppointmentProjectors;
using DentalClinic.Appointments.ReadModel.Appointments;
using DentalClinic.Appointments.ReadModel.IntegrationEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DentalClinic.Appointments.Projections.Worker;

public sealed class AppointmentProjectionsWorker : BackgroundService
{
    private readonly ServiceBusProcessor _processor;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<AppointmentProjectionsWorker> _logger;

    public AppointmentProjectionsWorker(
        ServiceBusProcessor processor,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<AppointmentProjectionsWorker> logger)
    {
        _processor = processor;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<ReadAppointmentsDbContext>();

            await dbContext.Database.EnsureCreatedAsync(stoppingToken);
        }

        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;

        await _processor.StartProcessingAsync(stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _processor.ProcessMessageAsync -= ProcessMessageAsync;
        _processor.ProcessErrorAsync -= ProcessErrorAsync;

        await _processor.StopProcessingAsync(cancellationToken);

        await base.StopAsync(cancellationToken);
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        var message = args.Message;

        try
        {
            if (!message.ApplicationProperties.TryGetValue(
                    "EventType",
                    out var eventTypeValue) ||
                eventTypeValue is not string eventType)
            {
                throw new InvalidOperationException(
                    "The message does not contain a string 'EventType' property.");
            }

            using var scope = _serviceScopeFactory.CreateScope();

            var dispatcher = scope.ServiceProvider
                .GetRequiredService<IntegrationEventDispatcher>();

            var projector = scope.ServiceProvider
                .GetRequiredService<IAppointmentProjector>();

            var integrationEvent = dispatcher.Deserialize(
                eventType,
                message.Body.ToString());

            await projector.ProjectAsync(
                integrationEvent,
                args.CancellationToken);

            await args.CompleteMessageAsync(
                message,
                args.CancellationToken);

            _logger.LogInformation(
                "Applied integration event {EventType} for message {MessageId}.",
                eventType,
                message.MessageId);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to process message {MessageId}.",
                message.MessageId);

            await args.AbandonMessageAsync(
                message,
                null,
                args.CancellationToken);
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(
            args.Exception,
            "Service Bus error while receiving: source {ErrorSource}.",
            args.ErrorSource);

        return Task.CompletedTask;
    }
}