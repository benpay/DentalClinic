using DentalClinic.Appointments.Application.Abstractions.Persistence;
using DentalClinic.Appointments.Application.Features.Appointments.Queries.GetAppointmentById;
using DentalClinic.Appointments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.Appointments.Infrastructure.Repositories;

public sealed class AppointmentReadRepository
    : IAppointmentReadRepository
{
    private readonly AppointmentsDbContext _dbContext;

    public AppointmentReadRepository(AppointmentsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AppointmentDetails?> GetByIdAsync(
        Guid appointmentId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Appointments
            .AsNoTracking()
            .Where(appointment => appointment.Id == appointmentId)
            .Select(appointment => new AppointmentDetails(
                appointment.Id,
                appointment.PatientId,
                appointment.DentistId,
                appointment.StartsAt,
                appointment.EndsAt,
                appointment.Status))
            .SingleOrDefaultAsync(cancellationToken);
    }
}