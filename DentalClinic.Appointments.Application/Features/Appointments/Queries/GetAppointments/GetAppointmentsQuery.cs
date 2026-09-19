using DentalClinic.Appointments.Application.Abstractions.Messaging;
using DentalClinic.Appointments.Application.Features.Appointments;

namespace DentalClinic.Appointments.Application.Features.Appointments.Queries.GetAppointments;

/// <summary>
/// Lists appointment projections, optionally filtered by dentist and/or a
/// start-time range. Read model is eventually consistent with the write side.
/// </summary>
public sealed record GetAppointmentsQuery(
    Guid? DentistId,
    DateTimeOffset? From,
    DateTimeOffset? To)
    : IQuery<IReadOnlyList<AppointmentDetails>>;