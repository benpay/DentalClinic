using FluentValidation;

namespace DentalClinic.Appointments.Application.Features.Appointments.Commands.ScheduleAppointment;

public sealed class ScheduleAppointmentCommandValidator
    : AbstractValidator<ScheduleAppointmentCommand>
{
    public ScheduleAppointmentCommandValidator()
    {
        RuleFor(command => command.PatientId)
            .NotEmpty();

        RuleFor(command => command.DentistId)
            .NotEmpty();

        RuleFor(command => command.EndsAt)
            .GreaterThan(command => command.StartsAt);
    }
}