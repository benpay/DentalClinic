using DentalClinic.Appointments.Application.Abstractions.Messaging;

namespace DentalClinic.Appointments.ReadModel.AppointmentProjectors;

public interface IAppointmentProjector
{
    Task ProjectAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default);
}