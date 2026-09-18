namespace DentalClinic.Appointments.Api.Contracts.Appointments;

public sealed record RescheduleAppointmentRequest(
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt);