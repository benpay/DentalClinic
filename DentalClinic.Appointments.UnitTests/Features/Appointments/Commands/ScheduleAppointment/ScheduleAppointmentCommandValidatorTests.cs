using DentalClinic.Appointments.Application.Features.Appointments.Commands.ScheduleAppointment;

namespace DentalClinic.Appointments.UnitTests.Features.Appointments.Commands.ScheduleAppointment;

public sealed class ScheduleAppointmentCommandValidatorTests
{
    private readonly ScheduleAppointmentCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_IsValid()
    {
        var result = _validator.Validate(CreateValidCommand());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenPatientIdIsEmpty_IsInvalid()
    {
        var command = CreateValidCommand() with
        {
            PatientId = Guid.Empty
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenDentistIdIsEmpty_IsInvalid()
    {
        var command = CreateValidCommand() with
        {
            DentistId = Guid.Empty
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenEndTimeIsNotLaterThanStartTime_IsInvalid()
    {
        var command = CreateValidCommand() with
        {
            EndsAt = new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero)
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    private static ScheduleAppointmentCommand CreateValidCommand()
    {
        return new ScheduleAppointmentCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 1, 9, 30, 0, TimeSpan.Zero));
    }
}