namespace DentalClinic.Appointments.ReadModel.Inbox;

/// <summary>
/// Consumer-side deduplication registry (Inbox pattern). Event ids recorded
/// here are skipped on redelivery so at-least-once messaging has no side effects.
/// </summary>
public interface IInbox
{
    Task<bool> HasProcessedAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);

    Task RecordProcessedAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);
}