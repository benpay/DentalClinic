using DentalClinic.Appointments.Domain.Appointments;

namespace DentalClinic.Appointments.UnitTests.Domain;

public sealed class AppointmentTests
{
    [Fact]
    public void Create_WithValidTimes_CreatesScheduledAppointment()
    {
        var startsAt = new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero);
        var endsAt = new DateTimeOffset(2026, 10, 1, 9, 30, 0, TimeSpan.Zero);

        var appointment = Appointment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            startsAt,
            endsAt);

        Assert.Equal(AppointmentStatus.Scheduled, appointment.Status);
        Assert.Equal(startsAt, appointment.StartsAt);
        Assert.Equal(endsAt, appointment.EndsAt);
    }

    [Fact]
    public void Create_WhenEndTimeIsNotLaterThanStartTime_ThrowsException()
    {
        var startsAt = new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero);

        var action = () => Appointment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            startsAt,
            startsAt);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Create_GeneratesNonEmptyId()
    {
        var appointment = CreateAppointment();

        Assert.NotEqual(Guid.Empty, appointment.Id);
    }

    [Fact]
    public void Create_SetsInitialVersionToOne()
    {
        var appointment = CreateAppointment();

        Assert.Equal(1, appointment.Version);
    }

    [Fact]
    public void Reschedule_IncrementsVersion()
    {
        var appointment = CreateAppointment();

        appointment.Reschedule(
            new DateTimeOffset(2026, 10, 2, 10, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 2, 10, 30, 0, TimeSpan.Zero));

        Assert.Equal(2, appointment.Version);
    }

    [Fact]
    public void Cancel_IncrementsVersion()
    {
        var appointment = CreateAppointment();

        appointment.Cancel();

        Assert.Equal(2, appointment.Version);
    }

    [Theory]
    [InlineData(9, 15, 9, 45, true)]
    [InlineData(9, 0, 9, 30, true)]
    [InlineData(8, 0, 10, 0, true)]
    [InlineData(9, 30, 10, 0, false)]
    [InlineData(10, 0, 10, 30, false)]
    [InlineData(9, 30, 9, 45, false)]
    public void TimePeriodsOverlap_EvaluatesRangeOverlap(
        int startHourB,
        int startMinuteB,
        int endHourB,
        int endMinuteB,
        bool expected)
    {
        var startsAt = new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero);
        var endsAt = new DateTimeOffset(2026, 10, 1, 9, 30, 0, TimeSpan.Zero);

        var otherStartsAt = new DateTimeOffset(
            2026, 10, 1, startHourB, startMinuteB, 0, TimeSpan.Zero);
        var otherEndsAt = new DateTimeOffset(
            2026, 10, 1, endHourB, endMinuteB, 0, TimeSpan.Zero);

        var overlaps = Appointment.TimePeriodsOverlap(
            startsAt,
            endsAt,
            otherStartsAt,
            otherEndsAt);

        Assert.Equal(expected, overlaps);
    }

    [Fact]
    public void Cancel_ChangesStatusToCancelled()
    {
        var appointment = CreateAppointment();

        appointment.Cancel();

        Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
    }

    [Fact]
    public void Reschedule_AfterCancellation_UpdatesTimesAndSchedulesAppointment()
    {
        var appointment = CreateAppointment();
        appointment.Cancel();

        var newStartsAt = new DateTimeOffset(2026, 10, 2, 10, 0, 0, TimeSpan.Zero);
        var newEndsAt = new DateTimeOffset(2026, 10, 2, 10, 30, 0, TimeSpan.Zero);

        appointment.Reschedule(newStartsAt, newEndsAt);

        Assert.Equal(AppointmentStatus.Scheduled, appointment.Status);
        Assert.Equal(newStartsAt, appointment.StartsAt);
        Assert.Equal(newEndsAt, appointment.EndsAt);
    }

    [Fact]
    public void Reschedule_WhenEndTimeIsNotLaterThanStartTime_ThrowsException()
    {
        var appointment = CreateAppointment();

        var startsAt = new DateTimeOffset(2026, 10, 2, 10, 0, 0, TimeSpan.Zero);

        Assert.Throws<ArgumentException>(
            () => appointment.Reschedule(startsAt, startsAt));
    }

    private static Appointment CreateAppointment()
    {
        return Appointment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 1, 9, 30, 0, TimeSpan.Zero));
    }
}