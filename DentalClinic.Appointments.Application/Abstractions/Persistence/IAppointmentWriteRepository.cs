using DentalClinic.Appointments.Domain.Appointments;

namespace DentalClinic.Appointments.Application.Abstractions.Persistence;

public interface IAppointmentWriteRepository
{
    Task AddAsync(
        Appointment appointment,
        CancellationToken cancellationToken = default);

    Task<Appointment?> GetForUpdateAsync(
        Guid appointmentId,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Appointment appointment,
        CancellationToken cancellationToken = default);
}