using System.Text.Json;
using DentalClinic.Appointments.Application.Abstractions.Messaging;

namespace DentalClinic.Appointments.Infrastructure.Persistence;

public sealed class OutboxMessage
{
    private OutboxMessage()
    {
    }

    private OutboxMessage(
        Guid id,
        DateTimeOffset occurredOnUtc,
        string type,
        string content)
    {
        Id = id;
        OccurredOnUtc = occurredOnUtc;
        Type = type;
        Content = content;
    }

    public Guid Id { get; private set; }

    public DateTimeOffset OccurredOnUtc { get; private set; }

    public string Type { get; private set; } = string.Empty;

    public string Content { get; private set; } = string.Empty;

    public DateTimeOffset? ProcessedOnUtc { get; private set; }

    public string? Error { get; private set; }

    public static OutboxMessage Create(IIntegrationEvent integrationEvent)
    {
        var eventType = integrationEvent.GetType();

        return new OutboxMessage(
            integrationEvent.EventId,
            integrationEvent.OccurredOnUtc,
            eventType.AssemblyQualifiedName
                ?? throw new InvalidOperationException(
                    "The integration event type could not be resolved."),
            JsonSerializer.Serialize(integrationEvent, eventType));
    }

    public void MarkAsProcessed(DateTimeOffset processedOnUtc)
    {
        ProcessedOnUtc = processedOnUtc;
        Error = null;
    }

    public void MarkAsFailed(string error)
    {
        Error = error;
    }
}