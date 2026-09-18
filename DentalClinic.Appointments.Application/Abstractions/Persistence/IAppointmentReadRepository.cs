using DentalClinic.Appointments.Application.Features.Appointments.Queries.GetAppointmentById;

namespace DentalClinic.Appointments.Application.Abstractions.Persistence;

public interface IAppointmentReadRepository
{
    Task<AppointmentDetails?> GetByIdAsync(
        Guid appointmentId,
        CancellationToken cancellationToken = default);
}