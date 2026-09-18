using DentalClinic.Appointments.Application.Abstractions.Messaging;

namespace DentalClinic.Appointments.Application.Features.Appointments.Commands.CancelAppointment;

public sealed record CancelAppointmentCommand(Guid AppointmentId) : ICommand;