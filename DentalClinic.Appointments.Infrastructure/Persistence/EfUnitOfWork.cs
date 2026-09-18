using DentalClinic.Appointments.Application.Abstractions.Persistence;

namespace DentalClinic.Appointments.Infrastructure.Persistence;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly AppointmentsDbContext _dbContext;

    public EfUnitOfWork(AppointmentsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}