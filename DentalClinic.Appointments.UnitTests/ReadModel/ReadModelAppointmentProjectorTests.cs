using DentalClinic.Appointments.Application.Features.Appointments.IntegrationEvents;
using DentalClinic.Appointments.Domain.Appointments;
using DentalClinic.Appointments.ReadModel.AppointmentProjectors;
using DentalClinic.Appointments.ReadModel.Appointments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace DentalClinic.Appointments.UnitTests.ReadModel;

public sealed class ReadModelAppointmentProjectorTests
{
    private static ReadAppointmentsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ReadAppointmentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ReadAppointmentsDbContext(options);
    }

    private static ReadModelAppointmentProjector CreateProjector(
        ReadAppointmentsDbContext dbContext)
    {
        return new ReadModelAppointmentProjector(
            dbContext,
            NullLogger<ReadModelAppointmentProjector>.Instance);
    }

    [Fact]
    public async Task ProjectAsync_ScheduleEvent_CreatesProjection()
    {
        await using var dbContext = CreateDbContext();
        var projector = CreateProjector(dbContext);

        var scheduled = new AppointmentScheduledIntegrationEvent(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 1, 9, 30, 0, TimeSpan.Zero));

        await projector.ProjectAsync(scheduled);

        var projection = await dbContext.AppointmentProjections
            .SingleOrDefaultAsync(p => p.Id == scheduled.AppointmentId);

        Assert.NotNull(projection);
        Assert.Equal(scheduled.PatientId, projection.PatientId);
        Assert.Equal(scheduled.DentistId, projection.DentistId);
        Assert.Equal(scheduled.StartsAt, projection.StartsAt);
        Assert.Equal(scheduled.EndsAt, projection.EndsAt);
        Assert.Equal(AppointmentStatus.Scheduled, projection.Status);
        Assert.Equal(1, projection.Version);
    }

    [Fact]
    public async Task ProjectAsync_ScheduleEvent_WhenProjectionExists_UpdatesIt()
    {
        await using var dbContext = CreateDbContext();
        var projector = CreateProjector(dbContext);

        var id = Guid.NewGuid();

        dbContext.AppointmentProjections.Add(new AppointmentProjection
        {
            Id = id,
            PatientId = Guid.NewGuid(),
            DentistId = Guid.NewGuid(),
            StartsAt = new DateTimeOffset(2026, 9, 1, 8, 0, 0, TimeSpan.Zero),
            EndsAt = new DateTimeOffset(2026, 9, 1, 8, 30, 0, TimeSpan.Zero),
            Status = AppointmentStatus.Cancelled,
            Version = 2,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync();

        await projector.ProjectAsync(new AppointmentScheduledIntegrationEvent(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            id,
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 1, 9, 30, 0, TimeSpan.Zero)));

        var projection = await dbContext.AppointmentProjections
            .SingleOrDefaultAsync(p => p.Id == id);

        Assert.NotNull(projection);
        Assert.Equal(3, projection.Version);
        Assert.Equal(AppointmentStatus.Scheduled, projection.Status);
    }

    [Fact]
    public async Task ProjectAsync_RescheduleEvent_UpdatesTimes()
    {
        await using var dbContext = CreateDbContext();
        var projector = CreateProjector(dbContext);

        var id = Guid.NewGuid();

        dbContext.AppointmentProjections.Add(new AppointmentProjection
        {
            Id = id,
            PatientId = Guid.NewGuid(),
            DentistId = Guid.NewGuid(),
            StartsAt = new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero),
            EndsAt = new DateTimeOffset(2026, 10, 1, 9, 30, 0, TimeSpan.Zero),
            Status = AppointmentStatus.Scheduled,
            Version = 1,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync();

        var newStartsAt = new DateTimeOffset(2026, 10, 2, 10, 0, 0, TimeSpan.Zero);
        var newEndsAt = new DateTimeOffset(2026, 10, 2, 10, 30, 0, TimeSpan.Zero);

        await projector.ProjectAsync(new AppointmentRescheduledIntegrationEvent(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            id,
            newStartsAt,
            newEndsAt));

        var projection = await dbContext.AppointmentProjections
            .SingleOrDefaultAsync(p => p.Id == id);

        Assert.NotNull(projection);
        Assert.Equal(newStartsAt, projection.StartsAt);
        Assert.Equal(newEndsAt, projection.EndsAt);
        Assert.Equal(2, projection.Version);
    }

    [Fact]
    public async Task ProjectAsync_CancelEvent_SetsStatusCancelled()
    {
        await using var dbContext = CreateDbContext();
        var projector = CreateProjector(dbContext);

        var id = Guid.NewGuid();

        dbContext.AppointmentProjections.Add(new AppointmentProjection
        {
            Id = id,
            PatientId = Guid.NewGuid(),
            DentistId = Guid.NewGuid(),
            StartsAt = new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero),
            EndsAt = new DateTimeOffset(2026, 10, 1, 9, 30, 0, TimeSpan.Zero),
            Status = AppointmentStatus.Scheduled,
            Version = 1,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync();

        await projector.ProjectAsync(new AppointmentCancelledIntegrationEvent(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            id));

        var projection = await dbContext.AppointmentProjections
            .SingleOrDefaultAsync(p => p.Id == id);

        Assert.NotNull(projection);
        Assert.Equal(AppointmentStatus.Cancelled, projection.Status);
        Assert.Equal(2, projection.Version);
    }

    [Fact]
    public async Task ProjectAsync_SameScheduleEventTwice_KeepsSingleProjection()
    {
        await using var dbContext = CreateDbContext();
        var projector = CreateProjector(dbContext);

        var scheduled = new AppointmentScheduledIntegrationEvent(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 1, 9, 30, 0, TimeSpan.Zero));

        await projector.ProjectAsync(scheduled);
        await projector.ProjectAsync(scheduled);

        var projections = await dbContext.AppointmentProjections.ToListAsync();

        var projection = Assert.Single(projections);
        Assert.Equal(scheduled.AppointmentId, projection.Id);
        Assert.Equal(2, projection.Version);
    }

    [Fact]
    public async Task ProjectAsync_RescheduleEvent_WhenProjectionMissing_DoesNotThrow()
    {
        await using var dbContext = CreateDbContext();
        var projector = CreateProjector(dbContext);

        await projector.ProjectAsync(new AppointmentRescheduledIntegrationEvent(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            Guid.NewGuid(),
            new DateTimeOffset(2026, 10, 2, 10, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 2, 10, 30, 0, TimeSpan.Zero)));

        Assert.Empty(
            await dbContext.AppointmentProjections.ToListAsync());
    }
}