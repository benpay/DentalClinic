using DentalClinic.Appointments.Application.Features.Appointments.Commands.RescheduleAppointment;

namespace DentalClinic.Appointments.UnitTests.Features.Appointments.Commands.RescheduleAppointment;

public sealed class RescheduleAppointmentCommandValidatorTests
{
    private readonly RescheduleAppointmentCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_IsValid()
    {
        var result = _validator.Validate(CreateValidCommand());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenAppointmentIdIsEmpty_IsInvalid()
    {
        var command = CreateValidCommand() with
        {
            AppointmentId = Guid.Empty
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenEndTimeIsNotLaterThanStartTime_IsInvalid()
    {
        var command = CreateValidCommand() with
        {
            EndsAt = new DateTimeOffset(2026, 10, 2, 10, 0, 0, TimeSpan.Zero)
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    private static RescheduleAppointmentCommand CreateValidCommand()
    {
        return new RescheduleAppointmentCommand(
            Guid.NewGuid(),
            new DateTimeOffset(2026, 10, 2, 10, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 2, 10, 30, 0, TimeSpan.Zero));
    }
}