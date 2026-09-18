using DentalClinic.Appointments.Application.Abstractions.Messaging;
using DentalClinic.Appointments.Application.Abstractions.Persistence;
using DentalClinic.Appointments.Application.Features.Appointments;
using DentalClinic.Appointments.Application.Features.Appointments.IntegrationEvents;
using MediatR;

namespace DentalClinic.Appointments.Application.Features.Appointments.Commands.CancelAppointment;

public sealed class CancelAppointmentCommandHandler
    : IRequestHandler<CancelAppointmentCommand, Guid>
{
    private readonly IAppointmentWriteRepository _appointmentWriteRepository;
    private readonly IOutbox _outbox;
    private readonly IUnitOfWork _unitOfWork;

    public CancelAppointmentCommandHandler(
        IAppointmentWriteRepository appointmentWriteRepository,
        IOutbox outbox,
        IUnitOfWork unitOfWork)
    {
        _appointmentWriteRepository = appointmentWriteRepository;
        _outbox = outbox;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        CancelAppointmentCommand request,
        CancellationToken cancellationToken)
    {
        var appointment = await _appointmentWriteRepository.GetForUpdateAsync(
            request.AppointmentId,
            cancellationToken);

        if (appointment is null)
        {
            throw new AppointmentNotFoundException(request.AppointmentId);
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