using DentalClinic.Appointments.Application.Abstractions.Persistence;
using DentalClinic.Appointments.Application.Features.Appointments.Queries.GetAppointmentById;
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
}