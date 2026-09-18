using FluentValidation;

namespace DentalClinic.Appointments.Application.Features.Appointments.Commands.CancelAppointment;

public sealed class CancelAppointmentCommandValidator
    : AbstractValidator<CancelAppointmentCommand>
{
    public CancelAppointmentCommandValidator()
    {
        RuleFor(command => command.AppointmentId)
            .NotEmpty();
    }
}