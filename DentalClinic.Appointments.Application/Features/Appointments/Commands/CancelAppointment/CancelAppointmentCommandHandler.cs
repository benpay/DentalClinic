using DentalClinic.Appointments.Application.Abstractions.Messaging;
using DentalClinic.Appointments.Application.Abstractions.Persistence;
using DentalClinic.Appointments.Application.Features.Appointments;
using DentalClinic.Appointments.Application.Features.Appointments.IntegrationEvents;
using FluentValidation;

namespace DentalClinic.Appointments.Application.Features.Appointments.Commands.CancelAppointment;

public sealed class CancelAppointmentCommandHandler
    : ICommandHandler<CancelAppointmentCommand, Guid>
{
    private readonly IAppointmentWriteRepository _appointmentWriteRepository;
    private readonly IValidator<CancelAppointmentCommand> _validator;
    private readonly IOutbox _outbox;
    private readonly IUnitOfWork _unitOfWork;

    public CancelAppointmentCommandHandler(
        IAppointmentWriteRepository appointmentWriteRepository,
        IValidator<CancelAppointmentCommand> validator,
        IOutbox outbox,
        IUnitOfWork unitOfWork)
    {
        _appointmentWriteRepository = appointmentWriteRepository;
        _validator = validator;
        _outbox = outbox;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> HandleAsync(
        CancelAppointmentCommand command,
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

        appointment.Cancel();

        await _appointmentWriteRepository.UpdateAsync(
            appointment,
            cancellationToken);

        _outbox.Add(new AppointmentCancelledIntegrationEvent(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            appointment.Id));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return appointment.Id;
    }
}