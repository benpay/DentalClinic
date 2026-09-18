using FluentValidation;

namespace DentalClinic.Appointments.Application.Features.Appointments.Commands.RescheduleAppointment;

public sealed class RescheduleAppointmentCommandValidator
    : AbstractValidator<RescheduleAppointmentCommand>
{
    public RescheduleAppointmentCommandValidator()
    {
        RuleFor(command => command.AppointmentId)
            .NotEmpty();

        RuleFor(command => command.EndsAt)
            .GreaterThan(command => command.StartsAt);
    }
}