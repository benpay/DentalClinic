using DentalClinic.Appointments.Domain.Appointments;
using DentalClinic.Appointments.Infrastructure.Persistence;
using DentalClinic.Appointments.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.Appointments.IntegrationTests.Persistence;

public sealed class AppointmentConcurrencyTests
{
    [Fact]
    public async Task SaveChanges_WhenAnotherWriterUpdatedTheAppointment_ThrowsConcurrencyException()
    {
        var databaseName = Guid.NewGuid().ToString();

        var appointment = Appointment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 1, 9, 30, 0, TimeSpan.Zero));

        var options = new DbContextOptionsBuilder<AppointmentsDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        // Writer 1 creates the appointment.
        await using (var writer1 = new AppointmentsDbContext(options))
        {
            var repository1 = new AppointmentWriteRepository(writer1);

            await repository1.AddAsync(appointment);
            await writer1.SaveChangesAsync();
        }

        // Writer 2 loads the appointment and keeps the context alive so the
        // original concurrency token value is preserved.
        await using var writer2 = new AppointmentsDbContext(options);
        var repository2 = new AppointmentWriteRepository(writer2);

        var staleAppointment = await repository2.GetForUpdateAsync(
            appointment.Id);

        Assert.NotNull(staleAppointment);

        // Writer 1 wins the race and reschedules first.
        await using (var writer1 = new AppointmentsDbContext(options))
        {
            var repository1 = new AppointmentWriteRepository(writer1);

            var current = await repository1.GetForUpdateAsync(appointment.Id);
            current!.Reschedule(
                new DateTimeOffset(2026, 10, 2, 10, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 10, 2, 10, 30, 0, TimeSpan.Zero));

            await writer1.SaveChangesAsync();
        }

        // Writer 2 now tries to persist its stale change -> concurrency conflict.
        staleAppointment.Reschedule(
            new DateTimeOffset(2026, 10, 3, 11, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 3, 11, 30, 0, TimeSpan.Zero));

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            () => writer2.SaveChangesAsync());
    }
}