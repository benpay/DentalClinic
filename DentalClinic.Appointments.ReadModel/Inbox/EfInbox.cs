using DentalClinic.Appointments.ReadModel.Appointments;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.Appointments.ReadModel.Inbox;

public sealed class EfInbox : IInbox
{
    private readonly ReadAppointmentsDbContext _dbContext;

    public EfInbox(ReadAppointmentsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> HasProcessedAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.InboxMessages.AnyAsync(
            message => message.EventId == eventId,
            cancellationToken);
    }

    public async Task RecordProcessedAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        if (await HasProcessedAsync(eventId, cancellationToken))
        {
            return;
        }

        _dbContext.InboxMessages.Add(
            InboxMessage.Create(eventId, DateTimeOffset.UtcNow));

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}