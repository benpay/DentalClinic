using Azure.Messaging.ServiceBus;
using DentalClinic.Appointments.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace DentalClinic.Appointments.Infrastructure.Messaging;

public sealed class AzureServiceBusOutboxPublisher
{
    private readonly ServiceBusSender _sender;
    private readonly ILogger<AzureServiceBusOutboxPublisher> _logger;

    public AzureServiceBusOutboxPublisher(
        ServiceBusSender sender,
        ILogger<AzureServiceBusOutboxPublisher> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task PublishAsync(
        OutboxMessage outboxMessage,
        CancellationToken cancellationToken = default)
    {
        var message = new ServiceBusMessage(outboxMessage.Content)
        {
            MessageId = outboxMessage.Id.ToString(),
            ContentType = "application/json"
        };

        message.ApplicationProperties["EventType"] = outboxMessage.Type;
        message.ApplicationProperties["OccurredOnUtc"] =
            outboxMessage.OccurredOnUtc.ToString("O");

        await _sender.SendMessageAsync(message, cancellationToken);

        _logger.LogInformation(
            "Sent message {MessageId} ({EventType}) to Service Bus topic.",
            message.MessageId,
            outboxMessage.Type);
    }
}