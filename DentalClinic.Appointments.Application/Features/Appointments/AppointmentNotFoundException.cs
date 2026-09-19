namespace DentalClinic.Appointments.Application.Features.Appointments;

public sealed class AppointmentNotFoundException : Exception
{
    public AppointmentNotFoundException(Guid appointmentId)
        : base($"Appointment '{appointmentId}' was not found.")
    {
        AppointmentId = appointmentId;
    }

    public Guid AppointmentId { get; }
}