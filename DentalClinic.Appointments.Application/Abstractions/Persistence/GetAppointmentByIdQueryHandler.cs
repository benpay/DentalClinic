using DentalClinic.Appointments.Application.Abstractions.Messaging;
using DentalClinic.Appointments.Application.Abstractions.Persistence;
using MediatR;

namespace DentalClinic.Appointments.Application.Features.Appointments.Queries.GetAppointmentById;

public sealed class GetAppointmentByIdQueryHandler
    : IRequestHandler<GetAppointmentByIdQuery, AppointmentDetails?>
{
    private readonly IAppointmentReadRepository _appointmentReadRepository;

    public GetAppointmentByIdQueryHandler(
        IAppointmentReadRepository appointmentReadRepository)
    {
        _appointmentReadRepository = appointmentReadRepository;
    }

    public Task<AppointmentDetails?> Handle(
        GetAppointmentByIdQuery request,
        CancellationToken cancellationToken)
    {
        return _appointmentReadRepository.GetByIdAsync(
            request.AppointmentId,
            cancellationToken);
    }
}