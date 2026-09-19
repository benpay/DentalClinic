using DentalClinic.Appointments.Application.Abstractions.Persistence;
using DentalClinic.Appointments.Application.Features.Appointments;
using DentalClinic.Appointments.ReadModel.Appointments;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.Appointments.ReadModel.Repositories;

public sealed class ReadModelAppointmentRepository
    : IAppointmentReadRepository
{
    private readonly ReadAppointmentsDbContext _dbContext;

    public ReadModelAppointmentRepository(ReadAppointmentsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AppointmentDetails?> GetByIdAsync(
        Guid appointmentId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.AppointmentProjections
            .AsNoTracking()
            .Where(projection => projection.Id == appointmentId)
            .Select(projection => new AppointmentDetails(
                projection.Id,
                projection.PatientId,
                projection.DentistId,
                projection.StartsAt,
                projection.EndsAt,
                projection.Status))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AppointmentDetails>> GetByFilterAsync(
        Guid? dentistId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AppointmentProjections
            .AsNoTracking()
            .AsQueryable();

        if (dentistId.HasValue)
        {
            query = query.Where(projection => projection.DentistId == dentistId.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(projection => projection.StartsAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(projection => projection.StartsAt <= to.Value);
        }

        var projections = await query
            .OrderBy(projection => projection.StartsAt)
            .ToListAsync(cancellationToken);

        return projections
            .Select(projection => new AppointmentDetails(
                projection.Id,
                projection.PatientId,
                projection.DentistId,
                projection.StartsAt,
                projection.EndsAt,
                projection.Status))
            .ToArray();
    }
}