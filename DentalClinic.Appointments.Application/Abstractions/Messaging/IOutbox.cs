namespace DentalClinic.Appointments.Application.Abstractions.Messaging;

public interface IOutbox
{
    void Add(IIntegrationEvent integrationEvent);
}