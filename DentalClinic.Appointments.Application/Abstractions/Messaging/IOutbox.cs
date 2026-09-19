namespace DentalClinic.Appointments.Application.Abstractions.Messaging;

/// <summary>
/// Collects integration events to be dispatched to the message broker once the
/// current transaction commits (transactional outbox).
/// </summary>
public interface IOutbox
{
    /// <summary>
    /// Queues <paramref name="integrationEvent"/> for publication.
    /// <paramref name="aggregateId"/> identifies the aggregate the event
    /// belongs to and is used as the message session (ordering key).
    /// </summary>
    void Add(Guid aggregateId, IIntegrationEvent integrationEvent);
}