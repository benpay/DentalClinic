using System.Text.Json;
using DentalClinic.Appointments.Application.Abstractions.Messaging;
using DentalClinic.Appointments.Application.Features.Appointments.IntegrationEvents;

namespace DentalClinic.Appointments.Infrastructure.Persistence;

public sealed class OutboxMessage
{
    private OutboxMessage()
    {
    }

    private OutboxMessage(
        Guid id,
        Guid aggregateId,
        DateTimeOffset occurredOnUtc,
        string type,
        string content)
    {
        Id = id;
        AggregateId = aggregateId;
        OccurredOnUtc = occurredOnUtc;
        Type = type;
        Content = content;
    }

    public Guid Id { get; private set; }

    public Guid AggregateId { get; private set; }

    public DateTimeOffset OccurredOnUtc { get; private set; }

    public string Type { get; private set; } = string.Empty;

    public string Content { get; private set; } = string.Empty;

    public DateTimeOffset? ProcessedOnUtc { get; private set; }

    public string? Error { get; private set; }

    public static OutboxMessage Create(
        Guid aggregateId,
        IIntegrationEvent integrationEvent)
    {
        var typeName = IntegrationEventTypeMap.GetNameFor(
            integrationEvent.GetType());

        return new OutboxMessage(
            integrationEvent.EventId,
            aggregateId,
            integrationEvent.OccurredOnUtc,
            typeName,
            JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType()));
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