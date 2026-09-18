using DentalClinic.Appointments.Application.Abstractions.Messaging;

namespace DentalClinic.Appointments.Application.Features.Appointments.IntegrationEvents;

public sealed record AppointmentCancelledIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredOnUtc,
    Guid AppointmentId) : IIntegrationEvent;