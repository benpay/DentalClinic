using Azure.Messaging.ServiceBus;
using DentalClinic.Appointments.ReadModel.AppointmentProjectors;
using DentalClinic.Appointments.ReadModel.IntegrationEvents;
using Microsoft.Extensions.DependencyInjection;

namespace DentalClinic.Appointments.Projections.Worker;

public sealed class AppointmentProjectionsWorker : BackgroundService
{
    private readonly ServiceBusSessionProcessor _processor;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<AppointmentProjectionsWorker> _logger;

    public AppointmentProjectionsWorker(
        ServiceBusSessionProcessor processor,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<AppointmentProjectionsWorker> logger)
    {
        _processor = processor;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor.ProcessMessageAsync += ProcessSessionMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;

        await _processor.StartProcessingAsync(stoppingToken);

        _logger.LogInformation(
            "Worker started (session processor) and listening for projections.");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _processor.ProcessMessageAsync -= ProcessSessionMessageAsync;
        _processor.ProcessErrorAsync -= ProcessErrorAsync;

        await _processor.StopProcessingAsync(cancellationToken);

        await base.StopAsync(cancellationToken);
    }

    private async Task ProcessSessionMessageAsync(
        ProcessSessionMessageEventArgs args)
    {
        var message = args.Message;
        var sessionId = args.SessionId;

        try
        {
            _logger.LogInformation(
                "Message {MessageId} received (session {SessionId}, sequence {SequenceNumber}, " +
                "enqueued {EnqueuedTime}).",
                message.MessageId,
                sessionId,
                message.SequenceNumber,
                message.EnqueuedTime);

            if (!message.ApplicationProperties.TryGetValue(
                    "EventType",
                    out var eventTypeValue) ||
                eventTypeValue is not string eventType)
            {
                throw new InvalidOperationException(
                    "The message does not contain a string 'EventType' property.");
            }

            _logger.LogInformation(
                "Dispatching integration event {EventType} for message {MessageId}.",
                eventType,
                message.MessageId);

            using var scope = _serviceScopeFactory.CreateScope();

            var dispatcher = scope.ServiceProvider
                .GetRequiredService<IntegrationEventDispatcher>();

            var projector = scope.ServiceProvider
                .GetRequiredService<IAppointmentProjector>();

            var integrationEvent = dispatcher.Deserialize(
                eventType,
                message.Body.ToString());

            _logger.LogInformation(
                "Projecting integration event {EventType} for message {MessageId}.",
                eventType,
                message.MessageId);

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
                "Failed to process message {MessageId} (session {SessionId}).",
                message.MessageId,
                sessionId);

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
            "Service Bus session error while receiving: source {ErrorSource}.",
            args.ErrorSource);

        return Task.CompletedTask;
    }
}