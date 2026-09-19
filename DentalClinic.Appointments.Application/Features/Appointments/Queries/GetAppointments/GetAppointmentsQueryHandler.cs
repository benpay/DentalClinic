using DentalClinic.Appointments.Application.Abstractions;
using DentalClinic.Appointments.Application.Abstractions.Persistence;
using DentalClinic.Appointments.Application.Features.Appointments;
using MediatR;

namespace DentalClinic.Appointments.Application.Features.Appointments.Queries.GetAppointments;

public sealed class GetAppointmentsQueryHandler
    : IRequestHandler<GetAppointmentsQuery, PagedResult<AppointmentDetails>>
{
    private readonly IAppointmentReadRepository _appointmentReadRepository;

    public GetAppointmentsQueryHandler(
        IAppointmentReadRepository appointmentReadRepository)
    {
        _appointmentReadRepository = appointmentReadRepository;
    }

    public Task<PagedResult<AppointmentDetails>> Handle(
        GetAppointmentsQuery request,
        CancellationToken cancellationToken)
    {
        return _appointmentReadRepository.GetByFilterAsync(
            request.DentistId,
            request.From,
            request.To,
            request.Page,
            request.PageSize,
            cancellationToken);
    }
}