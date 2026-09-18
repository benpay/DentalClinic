using DentalClinic.Appointments.Domain.Appointments;

namespace DentalClinic.Appointments.ReadModel.Appointments;

public sealed class AppointmentProjection
{
    public Guid Id { get; set; }

    public Guid PatientId { get; set; }

    public Guid DentistId { get; set; }

    public DateTimeOffset StartsAt { get; set; }

    public DateTimeOffset EndsAt { get; set; }

    public AppointmentStatus Status { get; set; }

    public long Version { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}