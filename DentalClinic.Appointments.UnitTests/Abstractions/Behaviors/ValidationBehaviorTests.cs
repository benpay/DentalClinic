using DentalClinic.Appointments.Application.Abstractions.Behaviors;
using DentalClinic.Appointments.Application.Features.Appointments;
using DentalClinic.Appointments.Application.Features.Appointments.Commands.ScheduleAppointment;
using DentalClinic.Appointments.Application.Features.Appointments.Queries.GetAppointmentById;
using FluentValidation;

namespace DentalClinic.Appointments.UnitTests.Abstractions.Behaviors;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WithValidCommand_InvokesNext()
    {
        var behavior = new ValidationBehavior<ScheduleAppointmentCommand, Guid>(
            [new ScheduleAppointmentCommandValidator()]);

        var nextWasCalled = false;

        var result = await behavior.Handle(
            CreateValidCommand(),
            _ =>
            {
                nextWasCalled = true;
                return Task.FromResult(Guid.NewGuid());
            },
            CancellationToken.None);

        Assert.True(nextWasCalled);
        Assert.NotEqual(Guid.Empty, result);
    }

    [Fact]
    public async Task Handle_WithInvalidCommand_ThrowsValidationExceptionAndDoesNotInvokeNext()
    {
        var behavior = new ValidationBehavior<ScheduleAppointmentCommand, Guid>(
            [new ScheduleAppointmentCommandValidator()]);

        var nextWasCalled = false;

        var invalidCommand = CreateValidCommand() with
        {
            PatientId = Guid.Empty
        };

        await Assert.ThrowsAsync<ValidationException>(
            async () => await behavior.Handle(
                invalidCommand,
                _ =>
                {
                    nextWasCalled = true;
                    return Task.FromResult(Guid.NewGuid());
                },
                CancellationToken.None));

        Assert.False(nextWasCalled);
    }

    [Fact]
    public async Task Handle_ForQuery_DoesNotValidateAndInvokesNext()
    {
        var behavior = new ValidationBehavior<GetAppointmentByIdQuery, AppointmentDetails?>(
            [new AlwaysFailingQueryValidator()]);

        var nextWasCalled = false;

        var result = await behavior.Handle(
            new GetAppointmentByIdQuery(Guid.Empty),
            _ =>
            {
                nextWasCalled = true;
                return Task.FromResult<AppointmentDetails?>(null);
            },
            CancellationToken.None);

        Assert.True(nextWasCalled);
        Assert.Null(result);
    }

    private static ScheduleAppointmentCommand CreateValidCommand()
    {
        return new ScheduleAppointmentCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 1, 9, 30, 0, TimeSpan.Zero));
    }

    private sealed class AlwaysFailingQueryValidator
        : AbstractValidator<GetAppointmentByIdQuery>
    {
        public AlwaysFailingQueryValidator()
        {
            RuleFor(query => query.AppointmentId).NotEmpty();
        }
    }
}