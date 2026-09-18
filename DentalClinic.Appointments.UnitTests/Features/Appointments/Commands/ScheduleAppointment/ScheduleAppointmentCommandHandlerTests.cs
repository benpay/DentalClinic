using DentalClinic.Appointments.Application.Abstractions.Messaging;
using DentalClinic.Appointments.Application.Abstractions.Persistence;
using DentalClinic.Appointments.Application.Features.Appointments.Commands.ScheduleAppointment;
using DentalClinic.Appointments.Application.Features.Appointments.IntegrationEvents;
using DentalClinic.Appointments.Domain.Appointments;

namespace DentalClinic.Appointments.UnitTests.Features.Appointments.Commands.ScheduleAppointment;

public sealed class ScheduleAppointmentCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_SavesAppointmentAndAddsOutboxEvent()
    {
        var repository = new FakeAppointmentWriteRepository();
        var outbox = new FakeOutbox();
        var unitOfWork = new FakeUnitOfWork();

        var handler = new ScheduleAppointmentCommandHandler(
            repository,
            outbox,
            unitOfWork);

        var patientId = Guid.NewGuid();
        var dentistId = Guid.NewGuid();
        var startsAt = new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero);
        var endsAt = new DateTimeOffset(2026, 10, 1, 9, 30, 0, TimeSpan.Zero);

        var command = new ScheduleAppointmentCommand(
            patientId,
            dentistId,
            startsAt,
            endsAt);

        var appointmentId = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.Single(repository.Appointments);

        var savedAppointment = repository.Appointments[0];

        Assert.Equal(appointmentId, savedAppointment.Id);
        Assert.Equal(patientId, savedAppointment.PatientId);
        Assert.Equal(dentistId, savedAppointment.DentistId);
        Assert.Equal(startsAt, savedAppointment.StartsAt);
        Assert.Equal(endsAt, savedAppointment.EndsAt);
        Assert.Equal(AppointmentStatus.Scheduled, savedAppointment.Status);

        Assert.Single(outbox.Events);

        var integrationEvent = Assert.IsType<AppointmentScheduledIntegrationEvent>(
            outbox.Events[0]);

        Assert.NotEqual(Guid.Empty, integrationEvent.EventId);
        Assert.Equal(appointmentId, integrationEvent.AppointmentId);
        Assert.Equal(patientId, integrationEvent.PatientId);
        Assert.Equal(dentistId, integrationEvent.DentistId);
        Assert.Equal(startsAt, integrationEvent.StartsAt);
        Assert.Equal(endsAt, integrationEvent.EndsAt);

        Assert.True(unitOfWork.SaveChangesWasCalled);
    }

    private sealed class FakeAppointmentWriteRepository
        : IAppointmentWriteRepository
    {
        public List<Appointment> Appointments { get; } = [];

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