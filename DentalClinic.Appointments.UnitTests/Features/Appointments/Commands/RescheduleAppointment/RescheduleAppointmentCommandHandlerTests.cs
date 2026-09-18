using DentalClinic.Appointments.Application.Abstractions.Messaging;
using DentalClinic.Appointments.Application.Abstractions.Persistence;
using DentalClinic.Appointments.Application.Features.Appointments;
using DentalClinic.Appointments.Application.Features.Appointments.Commands.RescheduleAppointment;
using DentalClinic.Appointments.Application.Features.Appointments.IntegrationEvents;
using DentalClinic.Appointments.Domain.Appointments;
using FluentValidation;

namespace DentalClinic.Appointments.UnitTests.Features.Appointments.Commands.RescheduleAppointment;

public sealed class RescheduleAppointmentCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithExistingCancelledAppointment_ReschedulesIt()
    {
        var repository = new FakeAppointmentWriteRepository();
        var appointment = CreateAppointment();
        var outbox = new FakeOutbox();
        var unitOfWork = new FakeUnitOfWork();
        appointment.Cancel();
        repository.Appointments.Add(appointment);

        var handler = new RescheduleAppointmentCommandHandler(
            repository,
            new RescheduleAppointmentCommandValidator(),
            outbox,
            unitOfWork);

        var newStartsAt = new DateTimeOffset(2026, 10, 2, 10, 0, 0, TimeSpan.Zero);
        var newEndsAt = new DateTimeOffset(2026, 10, 2, 10, 30, 0, TimeSpan.Zero);

        var command = new RescheduleAppointmentCommand(
            appointment.Id,
            newStartsAt,
            newEndsAt);

        var appointmentId = await handler.HandleAsync(command);
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
    public async Task HandleAsync_WithInvalidTimes_ThrowsValidationException()
    {
        var repository = new FakeAppointmentWriteRepository();
        var handler = new RescheduleAppointmentCommandHandler(
            repository,
            new RescheduleAppointmentCommandValidator(),
            new FakeOutbox(),
            new FakeUnitOfWork());

        var startsAt = new DateTimeOffset(2026, 10, 2, 10, 0, 0, TimeSpan.Zero);

        var command = new RescheduleAppointmentCommand(
            Guid.NewGuid(),
            startsAt,
            startsAt);

        await Assert.ThrowsAsync<ValidationException>(
            async () => await handler.HandleAsync(command));

        Assert.False(repository.UpdateWasCalled);
    }

    [Fact]
    public async Task HandleAsync_WhenAppointmentDoesNotExist_ThrowsNotFoundException()
    {
        var repository = new FakeAppointmentWriteRepository();
        var handler = new RescheduleAppointmentCommandHandler(
            repository,
            new RescheduleAppointmentCommandValidator(),
            new FakeOutbox(),
            new FakeUnitOfWork());

        var command = new RescheduleAppointmentCommand(
            Guid.NewGuid(),
            new DateTimeOffset(2026, 10, 2, 10, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 2, 10, 30, 0, TimeSpan.Zero));

        await Assert.ThrowsAsync<AppointmentNotFoundException>(
            async () => await handler.HandleAsync(command));
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
    }



    private sealed class FakeOutbox : IOutbox
    {
        public List<IIntegrationEvent> Events { get; } = [];

        public void Add(IIntegrationEvent integrationEvent)
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