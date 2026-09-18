using DentalClinic.Appointments.Application.Abstractions.Messaging;
using DentalClinic.Appointments.Application.Abstractions.Persistence;
using DentalClinic.Appointments.Application.Features.Appointments.IntegrationEvents;
using DentalClinic.Appointments.Domain.Appointments;
using FluentValidation;

namespace DentalClinic.Appointments.Application.Features.Appointments.Commands.ScheduleAppointment;

public sealed class ScheduleAppointmentCommandHandler
    : ICommandHandler<ScheduleAppointmentCommand, Guid>
{
    private readonly IAppointmentWriteRepository _appointmentWriteRepository;
    private readonly IValidator<ScheduleAppointmentCommand> _validator;
    private readonly IOutbox _outbox;
    private readonly IUnitOfWork _unitOfWork;

    public ScheduleAppointmentCommandHandler(
        IAppointmentWriteRepository appointmentWriteRepository,
        IValidator<ScheduleAppointmentCommand> validator,
        IOutbox outbox,
        IUnitOfWork unitOfWork)
    {
        _appointmentWriteRepository = appointmentWriteRepository;
        _validator = validator;
        _outbox = outbox;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> HandleAsync(
        ScheduleAppointmentCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var appointment = Appointment.Create(
            command.PatientId,
            command.DentistId,
            command.StartsAt,
            command.EndsAt);

        await _appointmentWriteRepository.AddAsync(
            appointment,
            cancellationToken);

        _outbox.Add(new AppointmentScheduledIntegrationEvent(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            appointment.Id,
            appointment.PatientId,
            appointment.DentistId,
            appointment.StartsAt,
            appointment.EndsAt));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return appointment.Id;
    }
}