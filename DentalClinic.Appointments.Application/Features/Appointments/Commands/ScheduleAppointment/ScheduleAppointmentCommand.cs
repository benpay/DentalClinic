using DentalClinic.Appointments.Application.Abstractions.Messaging;

namespace DentalClinic.Appointments.Application.Features.Appointments.Commands.ScheduleAppointment;

public sealed record ScheduleAppointmentCommand(
    Guid PatientId,
    Guid DentistId,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt) : ICommand;