namespace DentalClinic.Appointments.Application.Abstractions.Messaging;

public interface IIntegrationEvent
{
    Guid EventId { get; }

    DateTimeOffset OccurredOnUtc { get; }
}