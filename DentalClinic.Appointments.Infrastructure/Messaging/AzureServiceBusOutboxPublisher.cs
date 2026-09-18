using Azure.Messaging.ServiceBus;
using DentalClinic.Appointments.Infrastructure.Persistence;

namespace DentalClinic.Appointments.Infrastructure.Messaging;

public sealed class AzureServiceBusOutboxPublisher
{
    private readonly ServiceBusSender _sender;

    public AzureServiceBusOutboxPublisher(ServiceBusSender sender)
    {
        _sender = sender;
    }

    public Task PublishAsync(
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

        return _sender.SendMessageAsync(message, cancellationToken);
    }
}