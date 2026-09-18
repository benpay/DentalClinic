using DentalClinic.Appointments.Application.Features.Appointments.Commands.CancelAppointment;

namespace DentalClinic.Appointments.UnitTests.Features.Appointments.Commands.CancelAppointment;

public sealed class CancelAppointmentCommandValidatorTests
{
    private readonly CancelAppointmentCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_IsValid()
    {
        var result = _validator.Validate(
            new CancelAppointmentCommand(Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenAppointmentIdIsEmpty_IsInvalid()
    {
        var result = _validator.Validate(
            new CancelAppointmentCommand(Guid.Empty));

        Assert.False(result.IsValid);
    }
}