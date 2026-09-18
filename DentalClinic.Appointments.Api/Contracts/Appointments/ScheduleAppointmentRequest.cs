namespace DentalClinic.Appointments.Api.Contracts.Appointments;

public sealed record ScheduleAppointmentRequest(
    Guid PatientId,
    Guid DentistId,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt);