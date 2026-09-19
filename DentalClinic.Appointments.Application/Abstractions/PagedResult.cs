namespace DentalClinic.Appointments.Application.Abstractions;

public sealed record PagedResult<T>(
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyList<T> Items)
{
    public bool HasNextPage => Page * PageSize < TotalCount;
}