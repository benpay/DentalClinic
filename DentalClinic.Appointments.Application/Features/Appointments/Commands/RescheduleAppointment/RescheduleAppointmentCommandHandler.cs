using DentalClinic.Appointments.Application.Abstractions.Messaging;
using DentalClinic.Appointments.Application.Abstractions.Persistence;
using DentalClinic.Appointments.Application.Features.Appointments;
using DentalClinic.Appointments.Application.Features.Appointments.IntegrationEvents;
using MediatR;

namespace DentalClinic.Appointments.Application.Features.Appointments.Commands.RescheduleAppointment;

public sealed class RescheduleAppointmentCommandHandler
    : IRequestHandler<RescheduleAppointmentCommand, Guid>
{
    private readonly IAppointmentWriteRepository _appointmentWriteRepository;
    private readonly IOutbox _outbox;
    private readonly IUnitOfWork _unitOfWork;

    public RescheduleAppointmentCommandHandler(
        IAppointmentWriteRepository appointmentWriteRepository,
        IOutbox outbox,
        IUnitOfWork unitOfWork)
    {
        _appointmentWriteRepository = appointmentWriteRepository;
        _outbox = outbox;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        RescheduleAppointmentCommand request,
        CancellationToken cancellationToken)
    {
        var appointment = await _appointmentWriteRepository.GetForUpdateAsync(
            request.AppointmentId,
            cancellationToken);

        if (appointment is null)
        {
            throw new AppointmentNotFoundException(request.AppointmentId);
        }

        var hasOverlap = await _appointmentWriteRepository
            .HasOverlappingAppointmentAsync(
                appointment.DentistId,
                request.StartsAt,
                request.EndsAt,
                excludedAppointmentId: appointment.Id,
                cancellationToken);

        if (hasOverlap)
        {
            throw new AppointmentOverlapException(
                appointment.DentistId,
                request.StartsAt,
                request.EndsAt);
        }

        appointment.Reschedule(request.StartsAt, request.EndsAt);

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