using DentalClinic.Appointments.Application.Abstractions.Messaging;
using DentalClinic.Appointments.Application.Abstractions.Persistence;
using DentalClinic.Appointments.Application.Features.Appointments;
using DentalClinic.Appointments.Application.Features.Appointments.Commands.RescheduleAppointment;
using DentalClinic.Appointments.Application.Features.Appointments.IntegrationEvents;
using DentalClinic.Appointments.Domain.Appointments;

namespace DentalClinic.Appointments.UnitTests.Features.Appointments.Commands.RescheduleAppointment;

public sealed class RescheduleAppointmentCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithExistingCancelledAppointment_ReschedulesIt()
    {
        var repository = new FakeAppointmentWriteRepository();
        var appointment = CreateAppointment();
        var outbox = new FakeOutbox();
        var unitOfWork = new FakeUnitOfWork();
        appointment.Cancel();
        repository.Appointments.Add(appointment);

        var handler = new RescheduleAppointmentCommandHandler(
            repository,
            outbox,
            unitOfWork);

        var newStartsAt = new DateTimeOffset(2026, 10, 2, 10, 0, 0, TimeSpan.Zero);
        var newEndsAt = new DateTimeOffset(2026, 10, 2, 10, 30, 0, TimeSpan.Zero);

        var command = new RescheduleAppointmentCommand(
            appointment.Id,
            newStartsAt,
            newEndsAt);

        var appointmentId = await handler.Handle(
            command,
            CancellationToken.None);
        var integrationEvent = Assert.IsType<AppointmentRescheduledIntegrationEvent>(
            Assert.Single(outbox.Events));

        Assert.Equal(appointment.Id, integrationEvent.AppointmentId);
        Assert.Equal(newStartsAt, integrationEvent.StartsAt);
        Assert.Equal(newEndsAt, integrationEvent.EndsAt);
        Assert.True(unitOfWork.SaveChangesWasCalled);
        Assert.Equal(appointment.Id, appointmentId);
        Assert.Equal(newStartsAt, appointment.StartsAt);
        Assert.Equal(newEndsAt, appointment.EndsAt);
        Assert.Equal(AppointmentStatus.Scheduled, appointment.Status);
        Assert.True(repository.UpdateWasCalled);
    }

    [Fact]
    public async Task Handle_WhenAppointmentDoesNotExist_ThrowsNotFoundException()
    {
        var repository = new FakeAppointmentWriteRepository();
        var handler = new RescheduleAppointmentCommandHandler(
            repository,
            new FakeOutbox(),
            new FakeUnitOfWork());

        var command = new RescheduleAppointmentCommand(
            Guid.NewGuid(),
            new DateTimeOffset(2026, 10, 2, 10, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 2, 10, 30, 0, TimeSpan.Zero));

        await Assert.ThrowsAsync<AppointmentNotFoundException>(
            async () => await handler.Handle(
                command,
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenAnotherAppointmentOverlaps_ThrowsOverlapException()
    {
        var repository = new FakeAppointmentWriteRepository
        {
            HasOverlap = true
        };
        var appointment = CreateAppointment();
        repository.Appointments.Add(appointment);

        var handler = new RescheduleAppointmentCommandHandler(
            repository,
            new FakeOutbox(),
            new FakeUnitOfWork());

        var command = new RescheduleAppointmentCommand(
            appointment.Id,
            new DateTimeOffset(2026, 10, 2, 10, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 2, 10, 30, 0, TimeSpan.Zero));

        await Assert.ThrowsAsync<AppointmentOverlapException>(
            async () => await handler.Handle(
                command,
                CancellationToken.None));

        Assert.Equal(AppointmentStatus.Scheduled, appointment.Status);
    }

    private static Appointment CreateAppointment()
    {
        return Appointment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 1, 9, 30, 0, TimeSpan.Zero));
    }

    private sealed class FakeAppointmentWriteRepository
        : IAppointmentWriteRepository
    {
        public List<Appointment> Appointments { get; } = [];

        public bool UpdateWasCalled { get; private set; }

        public Task AddAsync(
            Appointment appointment,
            CancellationToken cancellationToken = default)
        {
            Appointments.Add(appointment);

            return Task.CompletedTask;
        }

        public Task<Appointment?> GetForUpdateAsync(
            Guid appointmentId,
            CancellationToken cancellationToken = default)
        {
            var appointment = Appointments.SingleOrDefault(
                appointment => appointment.Id == appointmentId);

            return Task.FromResult(appointment);
        }

        public Task UpdateAsync(
            Appointment appointment,
            CancellationToken cancellationToken = default)
        {
            UpdateWasCalled = true;

            return Task.CompletedTask;
        }

        public bool HasOverlap { get; set; }

        public Task<bool> HasOverlappingAppointmentAsync(
            Guid dentistId,
            DateTimeOffset startsAt,
            DateTimeOffset endsAt,
            Guid? excludedAppointmentId = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(HasOverlap);
        }
    }



    private sealed class FakeOutbox : IOutbox
    {
        public List<IIntegrationEvent> Events { get; } = [];

        public void Add(Guid aggregateId, IIntegrationEvent integrationEvent)
        {
            Events.Add(integrationEvent);
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public bool SaveChangesWasCalled { get; private set; }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesWasCalled = true;

            return Task.CompletedTask;
        }
    }
}