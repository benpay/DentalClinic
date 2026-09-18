using DentalClinic.Appointments.Application.Abstractions.Messaging;
using DentalClinic.Appointments.Application.Abstractions.Persistence;
using DentalClinic.Appointments.Application.Features.Appointments;
using DentalClinic.Appointments.Application.Features.Appointments.IntegrationEvents;
using FluentValidation;

namespace DentalClinic.Appointments.Application.Features.Appointments.Commands.RescheduleAppointment;

public sealed class RescheduleAppointmentCommandHandler
    : ICommandHandler<RescheduleAppointmentCommand, Guid>
{
    private readonly IAppointmentWriteRepository _appointmentWriteRepository;
    private readonly IValidator<RescheduleAppointmentCommand> _validator;
    private readonly IOutbox _outbox;
    private readonly IUnitOfWork _unitOfWork;

    public RescheduleAppointmentCommandHandler(
        IAppointmentWriteRepository appointmentWriteRepository,
        IValidator<RescheduleAppointmentCommand> validator,
        IOutbox outbox,
        IUnitOfWork unitOfWork)
    {
        _appointmentWriteRepository = appointmentWriteRepository;
        _validator = validator;
        _outbox = outbox;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> HandleAsync(
        RescheduleAppointmentCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var appointment = await _appointmentWriteRepository.GetForUpdateAsync(
            command.AppointmentId,
            cancellationToken);

        if (appointment is null)
        {
            throw new AppointmentNotFoundException(command.AppointmentId);
        }

        appointment.Reschedule(command.StartsAt, command.EndsAt);

        await _appointmentWriteRepository.UpdateAsync(
            appointment,
            cancellationToken);

        _outbox.Add(new AppointmentRescheduledIntegrationEvent(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            appointment.Id,
            appointment.StartsAt,
            appointment.EndsAt));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return appointment.Id;
    }
}