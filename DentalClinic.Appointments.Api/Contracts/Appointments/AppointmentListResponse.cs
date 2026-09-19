using DentalClinic.Appointments.Application.Features.Appointments;

namespace DentalClinic.Appointments.Api.Contracts.Appointments;

public sealed record AppointmentListResponse(
    int Page,
    int PageSize,
    int TotalCount,
    bool HasNextPage,
    IReadOnlyList<AppointmentDetails> Items);