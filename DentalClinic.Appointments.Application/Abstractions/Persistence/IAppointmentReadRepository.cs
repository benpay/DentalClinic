using DentalClinic.Appointments.Application.Features.Appointments;

namespace DentalClinic.Appointments.Application.Abstractions.Persistence;

/// <summary>
/// Read-side (query) access to appointment projections.
/// </summary>
/// <remarks>
/// The read model is kept eventually consistent with the write side through
/// the outbox + Service Bus pipeline. A 404 from <see cref="GetByIdAsync"/>
/// does not necessarily mean the appointment was never created: its projection
/// may still be propagating.
/// </remarks>
public interface IAppointmentReadRepository
{
    /// <summary>
    /// Returns the projection for a single appointment, or <c>null</c> when it
    /// has not been projected yet (eventual consistency).
    /// </summary>
    Task<AppointmentDetails?> GetByIdAsync(
        Guid appointmentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a page of projections matching the supplied filters, ordered by
    /// start time. All filters are optional; <c>From</c>/<c>To</c> bound
    /// <see cref="AppointmentDetails.StartsAt"/>.
    /// </summary>
    Task<PagedResult<AppointmentDetails>> GetByFilterAsync(
        Guid? dentistId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}