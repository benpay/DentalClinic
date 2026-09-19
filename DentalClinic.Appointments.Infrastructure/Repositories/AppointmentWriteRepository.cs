using DentalClinic.Appointments.Application.Abstractions.Persistence;
using DentalClinic.Appointments.Domain.Appointments;
using DentalClinic.Appointments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.Appointments.Infrastructure.Repositories;

public sealed class AppointmentWriteRepository
    : IAppointmentWriteRepository
{
    private readonly AppointmentsDbContext _dbContext;

    public AppointmentWriteRepository(AppointmentsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(
        Appointment appointment,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Appointments.Add(appointment);

        return Task.CompletedTask;
    }

    public Task<Appointment?> GetForUpdateAsync(
        Guid appointmentId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Appointments
            .SingleOrDefaultAsync(
                appointment => appointment.Id == appointmentId,
                cancellationToken);
    }

    public Task UpdateAsync(
        Appointment appointment,
        CancellationToken cancellationToken = default)
    {
        if (_dbContext.Entry(appointment).State == EntityState.Detached)
        {
            _dbContext.Appointments.Update(appointment);
        }

        return Task.CompletedTask;
    }

    public Task<bool> HasOverlappingAppointmentAsync(
        Guid dentistId,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        Guid? excludedAppointmentId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Appointments.AnyAsync(
            appointment =>
                appointment.DentistId == dentistId &&
                appointment.Status != AppointmentStatus.Cancelled &&
                appointment.Id != excludedAppointmentId &&
                appointment.StartsAt < endsAt &&
                startsAt < appointment.EndsAt,
            cancellationToken);
    }
}