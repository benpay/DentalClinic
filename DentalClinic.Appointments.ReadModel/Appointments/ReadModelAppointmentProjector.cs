using DentalClinic.Appointments.Application.Abstractions.Messaging;
using DentalClinic.Appointments.Application.Features.Appointments.IntegrationEvents;
using DentalClinic.Appointments.Domain.Appointments;
using DentalClinic.Appointments.ReadModel.Appointments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DentalClinic.Appointments.ReadModel.AppointmentProjectors;

public sealed class ReadModelAppointmentProjector : IAppointmentProjector
{
    private readonly ReadAppointmentsDbContext _dbContext;
    private readonly ILogger<ReadModelAppointmentProjector> _logger;

    public ReadModelAppointmentProjector(
        ReadAppointmentsDbContext dbContext,
        ILogger<ReadModelAppointmentProjector> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task ProjectAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        switch (integrationEvent)
        {
            case AppointmentScheduledIntegrationEvent scheduled:
                await ApplyScheduledAsync(scheduled, cancellationToken);
                break;

            case AppointmentRescheduledIntegrationEvent rescheduled:
                await ApplyRescheduledAsync(rescheduled, cancellationToken);
                break;

            case AppointmentCancelledIntegrationEvent cancelled:
                await ApplyCancelledAsync(cancelled, cancellationToken);
                break;

            default:
                _logger.LogWarning(
                    "Unhandled integration event type {EventType}.",
                    integrationEvent.GetType().Name);
                return;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task ApplyScheduledAsync(
        AppointmentScheduledIntegrationEvent scheduled,
        CancellationToken cancellationToken)
    {
        var projection = await FindByIdAsync(
            scheduled.AppointmentId,
            cancellationToken);

        if (projection is null)
        {
            _dbContext.AppointmentProjections.Add(new AppointmentProjection
            {
                Id = scheduled.AppointmentId,
                PatientId = scheduled.PatientId,
                DentistId = scheduled.DentistId,
                StartsAt = scheduled.StartsAt,
                EndsAt = scheduled.EndsAt,
                Status = AppointmentStatus.Scheduled,
                Version = 1,
                UpdatedAtUtc = scheduled.OccurredOnUtc
            });

            _logger.LogInformation(
                "Projection scheduled for appointment {AppointmentId} " +
                "(dentist {DentistId}, {StartsAt} - {EndsAt}).",
                scheduled.AppointmentId,
                scheduled.DentistId,
                scheduled.StartsAt,
                scheduled.EndsAt);

            return;
        }

        projection.PatientId = scheduled.PatientId;
        projection.DentistId = scheduled.DentistId;
        projection.StartsAt = scheduled.StartsAt;
        projection.EndsAt = scheduled.EndsAt;
        projection.Status = AppointmentStatus.Scheduled;
        projection.Version += 1;
        projection.UpdatedAtUtc = scheduled.OccurredOnUtc;

        _logger.LogInformation(
            "Projection updated for appointment {AppointmentId} " +
            "(re-scheduled event for an existing projection).",
            scheduled.AppointmentId);
    }

    private async Task ApplyRescheduledAsync(
        AppointmentRescheduledIntegrationEvent rescheduled,
        CancellationToken cancellationToken)
    {
        var projection = await FindByIdAsync(
            rescheduled.AppointmentId,
            cancellationToken);

        if (projection is null)
        {
            _logger.LogWarning(
                "Projection for appointment {AppointmentId} was not found; " +
                "ignoring reschedule until a schedule event is received.",
                rescheduled.AppointmentId);
            return;
        }

        projection.StartsAt = rescheduled.StartsAt;
        projection.EndsAt = rescheduled.EndsAt;
        projection.Status = AppointmentStatus.Scheduled;
        projection.Version += 1;
        projection.UpdatedAtUtc = rescheduled.OccurredOnUtc;

        _logger.LogInformation(
            "Projection rescheduled for appointment {AppointmentId} " +
            "({StartsAt} - {EndsAt}).",
            rescheduled.AppointmentId,
            rescheduled.StartsAt,
            rescheduled.EndsAt);
    }

    private async Task ApplyCancelledAsync(
        AppointmentCancelledIntegrationEvent cancelled,
        CancellationToken cancellationToken)
    {
        var projection = await FindByIdAsync(
            cancelled.AppointmentId,
            cancellationToken);

        if (projection is null)
        {
            _logger.LogWarning(
                "Projection for appointment {AppointmentId} was not found; " +
                "ignoring cancellation until a schedule event is received.",
                cancelled.AppointmentId);
            return;
        }

        projection.Status = AppointmentStatus.Cancelled;
        projection.Version += 1;
        projection.UpdatedAtUtc = cancelled.OccurredOnUtc;

        _logger.LogInformation(
            "Projection cancelled for appointment {AppointmentId}.",
            cancelled.AppointmentId);
    }

    private Task<AppointmentProjection?> FindByIdAsync(
        Guid appointmentId,
        CancellationToken cancellationToken)
    {
        return _dbContext.AppointmentProjections
            .SingleOrDefaultAsync(
                projection => projection.Id == appointmentId,
                cancellationToken);
    }
}