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
        Version = 1;
    }

    public Guid Id { get; private set; }

    public Guid PatientId { get; private set; }

    public Guid DentistId { get; private set; }

    public DateTimeOffset StartsAt { get; private set; }

    public DateTimeOffset EndsAt { get; private set; }

    public AppointmentStatus Status { get; private set; }

    public int Version { get; private set; }

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
        Version += 1;
    }

    public void Cancel()
    {
        Status = AppointmentStatus.Cancelled;
        Version += 1;
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

    public static bool TimePeriodsOverlap(
        DateTimeOffset startsAtA,
        DateTimeOffset endsAtA,
        DateTimeOffset startsAtB,
        DateTimeOffset endsAtB)
    {
        return startsAtA < endsAtB && startsAtB < endsAtA;
    }
}