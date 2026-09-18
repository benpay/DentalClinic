using DentalClinic.Appointments.Application.Abstractions.Messaging;

namespace DentalClinic.Appointments.Application.Features.Appointments.Queries.GetAppointmentById;

public sealed record GetAppointmentByIdQuery(Guid AppointmentId)
    : IQuery<AppointmentDetails?>;