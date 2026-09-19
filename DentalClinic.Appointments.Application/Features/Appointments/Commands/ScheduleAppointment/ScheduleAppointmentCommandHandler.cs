using DentalClinic.Appointments.Application.Abstractions.Messaging;
using DentalClinic.Appointments.Application.Abstractions.Persistence;
using DentalClinic.Appointments.Application.Features.Appointments.IntegrationEvents;
using DentalClinic.Appointments.Domain.Appointments;
using MediatR;

namespace DentalClinic.Appointments.Application.Features.Appointments.Commands.ScheduleAppointment;

public sealed class ScheduleAppointmentCommandHandler
    : IRequestHandler<ScheduleAppointmentCommand, Guid>
{
    private readonly IAppointmentWriteRepository _appointmentWriteRepository;
    private readonly IOutbox _outbox;
    private readonly IUnitOfWork _unitOfWork;

    public ScheduleAppointmentCommandHandler(
        IAppointmentWriteRepository appointmentWriteRepository,
        IOutbox outbox,
        IUnitOfWork unitOfWork)
    {
        _appointmentWriteRepository = appointmentWriteRepository;
        _outbox = outbox;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        ScheduleAppointmentCommand request,
        CancellationToken cancellationToken)
    {
        var hasOverlap = await _appointmentWriteRepository
            .HasOverlappingAppointmentAsync(
                request.DentistId,
                request.StartsAt,
                request.EndsAt,
                excludedAppointmentId: null,
                cancellationToken);

        if (hasOverlap)
        {
            throw new AppointmentOverlapException(
                request.DentistId,
                request.StartsAt,
                request.EndsAt);
        }

        var appointment = Appointment.Create(
            request.PatientId,
            request.DentistId,
            request.StartsAt,
            request.EndsAt);

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