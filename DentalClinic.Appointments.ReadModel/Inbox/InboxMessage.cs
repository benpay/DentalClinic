namespace DentalClinic.Appointments.ReadModel.Inbox;

public sealed class InboxMessage
{
    private InboxMessage()
    {
    }

    private InboxMessage(
        Guid eventId,
        DateTimeOffset processedOnUtc)
    {
        EventId = eventId;
        ProcessedOnUtc = processedOnUtc;
    }

    public Guid EventId { get; private set; }

    public DateTimeOffset ProcessedOnUtc { get; private set; }

    public static InboxMessage Create(
        Guid eventId,
        DateTimeOffset processedOnUtc)
    {
        return new InboxMessage(eventId, processedOnUtc);
    }
}