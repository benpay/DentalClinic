using DentalClinic.Appointments.ReadModel.Appointments;
using DentalClinic.Appointments.ReadModel.Inbox;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.Appointments.UnitTests.ReadModel;

public sealed class EfInboxTests
{
    private static ReadAppointmentsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ReadAppointmentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ReadAppointmentsDbContext(options);
    }

    [Fact]
    public async Task HasProcessedAsync_ForUnprocessedEvent_ReturnsFalse()
    {
        await using var dbContext = CreateDbContext();
        var inbox = new EfInbox(dbContext);

        var processed = await inbox.HasProcessedAsync(Guid.NewGuid());

        Assert.False(processed);
    }

    [Fact]
    public async Task RecordProcessedAsync_ThenHasProcessedAsync_ReturnsTrue()
    {
        await using var dbContext = CreateDbContext();
        var inbox = new EfInbox(dbContext);
        var eventId = Guid.NewGuid();

        await inbox.RecordProcessedAsync(eventId);
        await inbox.RecordProcessedAsync(eventId);

        Assert.True(await inbox.HasProcessedAsync(eventId));
        Assert.Equal(1, await dbContext.InboxMessages.CountAsync());
    }
}