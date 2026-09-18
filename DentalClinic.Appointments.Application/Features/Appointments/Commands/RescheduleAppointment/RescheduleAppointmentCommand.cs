using DentalClinic.Appointments.Application.Abstractions.Messaging;

namespace DentalClinic.Appointments.Application.Features.Appointments.Commands.RescheduleAppointment;

public sealed record RescheduleAppointmentCommand(
    Guid AppointmentId,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt) : ICommand<Guid>;