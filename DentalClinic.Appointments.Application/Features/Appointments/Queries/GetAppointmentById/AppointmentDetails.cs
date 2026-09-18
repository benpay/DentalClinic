using DentalClinic.Appointments.Domain.Appointments;

namespace DentalClinic.Appointments.Application.Features.Appointments.Queries.GetAppointmentById;

public sealed record AppointmentDetails(
    Guid Id,
    Guid PatientId,
    Guid DentistId,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    AppointmentStatus Status);