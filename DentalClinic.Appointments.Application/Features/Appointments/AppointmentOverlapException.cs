namespace DentalClinic.Appointments.Application.Features.Appointments;

public sealed class AppointmentOverlapException : Exception
{
    public AppointmentOverlapException(Guid dentistId, DateTimeOffset startsAt, DateTimeOffset endsAt)
        : base($"The dentist '{dentistId}' already has an overlap " +
               $"within '{startsAt:O}' and '{endsAt:O}'.")
    {
        DentistId = dentistId;
    }

    public Guid DentistId { get; }
}