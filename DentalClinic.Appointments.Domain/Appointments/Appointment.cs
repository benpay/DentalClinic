namespace DentalClinic.Appointments.Domain.Appointments;

public sealed class Appointment
{
    private Appointment() { }

    private Appointment(
        Guid id,
        Guid patientId,
        Guid dentistId,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt)
    {
        ValidateTimeRange(startsAt, endsAt);

        Id = id;
        PatientId = patientId;
        DentistId = dentistId;
        StartsAt = startsAt;
        EndsAt = endsAt;
        Status = AppointmentStatus.Scheduled;
    }

    public Guid Id { get; private set; }

    public Guid PatientId { get; private set; }

    public Guid DentistId { get; private set; }

    public DateTimeOffset StartsAt { get; private set; }

    public DateTimeOffset EndsAt { get; private set; }

    public AppointmentStatus Status { get; private set; }

    public static Appointment Create(
        Guid patientId,
        Guid dentistId,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt)
    {
        return new Appointment(
            Guid.NewGuid(),
            patientId,
            dentistId,
            startsAt,
            endsAt);
    }

    public void Reschedule(DateTimeOffset startsAt, DateTimeOffset endsAt)
    {
        ValidateTimeRange(startsAt, endsAt);

        StartsAt = startsAt;
        EndsAt = endsAt;
        Status = AppointmentStatus.Scheduled;
    }

    public void Cancel()
    {
        Status = AppointmentStatus.Cancelled;
    }

    private static void ValidateTimeRange(
        DateTimeOffset startsAt,
        DateTimeOffset endsAt)
    {
        if (endsAt <= startsAt)
        {
            throw new ArgumentException(
                "The appointment end time must be later than its start time.");
        }
    }
}