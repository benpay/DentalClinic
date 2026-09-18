using DentalClinic.Appointments.Application.Abstractions.Messaging;
using DentalClinic.Appointments.Application.Abstractions.Persistence;

namespace DentalClinic.Appointments.Application.Features.Appointments.Queries.GetAppointmentById;

public sealed class GetAppointmentByIdQueryHandler
    : IQueryHandler<GetAppointmentByIdQuery, AppointmentDetails?>
{
    private readonly IAppointmentReadRepository _appointmentReadRepository;

    public GetAppointmentByIdQueryHandler(
        IAppointmentReadRepository appointmentReadRepository)
    {
        _appointmentReadRepository = appointmentReadRepository;
    }

    public Task<AppointmentDetails?> HandleAsync(
        GetAppointmentByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        return _appointmentReadRepository.GetByIdAsync(
            query.AppointmentId,
            cancellationToken);
    }
}