using System.Text.Json;
using DentalClinic.Appointments.Application.Features.Appointments.IntegrationEvents;
using DentalClinic.Appointments.ReadModel.IntegrationEvents;

namespace DentalClinic.Appointments.UnitTests.ReadModel;

public sealed class IntegrationEventDispatcherTests
{
    [Fact]
    public void Deserialize_ScheduledEvent_RoundTrips()
    {
        var dispatcher = new IntegrationEventDispatcher();

        var scheduled = new AppointmentScheduledIntegrationEvent(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 1, 9, 30, 0, TimeSpan.Zero));

        var expectedTypeName =
            IntegrationEventTypeName.AppointmentScheduled;

        var json = JsonSerializer.Serialize(
            scheduled,
            scheduled.GetType());

        var result = dispatcher.Deserialize(expectedTypeName, json);

        var deserialized =
            Assert.IsType<AppointmentScheduledIntegrationEvent>(result);

        Assert.Equal(scheduled.EventId, deserialized.EventId);
        Assert.Equal(scheduled.AppointmentId, deserialized.AppointmentId);
        Assert.Equal(scheduled.PatientId, deserialized.PatientId);
        Assert.Equal(scheduled.DentistId, deserialized.DentistId);
        Assert.Equal(scheduled.StartsAt, deserialized.StartsAt);
        Assert.Equal(scheduled.EndsAt, deserialized.EndsAt);
    }

    [Fact]
    public void Deserialize_UnknownEventType_ThrowsNotSupportedException()
    {
        var dispatcher = new IntegrationEventDispatcher();

        Assert.Throws<NotSupportedException>(() =>
            dispatcher.Deserialize(
                "My.Namespace.UnknownEvent, My.Assembly",
                "{}"));
    }
}