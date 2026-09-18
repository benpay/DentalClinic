using DentalClinic.Appointments.Application.Abstractions.Messaging;

namespace DentalClinic.Appointments.Application.Features.Appointments.IntegrationEvents;

public sealed record AppointmentRescheduledIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredOnUtc,
    Guid AppointmentId,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt) : IIntegrationEvent;