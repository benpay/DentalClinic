using DentalClinic.Appointments.Application.Abstractions.Messaging;

namespace DentalClinic.Appointments.Application.Features.Appointments.IntegrationEvents;

public sealed record AppointmentScheduledIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredOnUtc,
    Guid AppointmentId,
    Guid PatientId,
    Guid DentistId,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt) : IIntegrationEvent;