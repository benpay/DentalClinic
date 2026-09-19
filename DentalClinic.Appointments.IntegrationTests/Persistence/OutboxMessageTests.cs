using DentalClinic.Appointments.Application.Features.Appointments.IntegrationEvents;
using DentalClinic.Appointments.Infrastructure.Persistence;
using DentalClinic.Appointments.ReadModel.IntegrationEvents;

namespace DentalClinic.Appointments.IntegrationTests.Persistence;

public sealed class OutboxMessageTests
{
    [Fact]
    public void Create_ThenDeserialize_RoundTripsThroughReadModelDispatcher()
    {
        var scheduled = new AppointmentScheduledIntegrationEvent(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 1, 9, 30, 0, TimeSpan.Zero));

        var message = OutboxMessage.Create(scheduled.AppointmentId, scheduled);

        Assert.NotNull(message.Type);
        Assert.Contains(
            typeof(AppointmentScheduledIntegrationEvent).FullName!,
            message.Type);
        Assert.NotEmpty(message.Content);

        var deserialized = new IntegrationEventDispatcher()
            .Deserialize(message.Type, message.Content);

        var roundTripped =
            Assert.IsType<AppointmentScheduledIntegrationEvent>(deserialized);

        Assert.Equal(scheduled.EventId, roundTripped.EventId);
        Assert.Equal(scheduled.OccurredOnUtc, roundTripped.OccurredOnUtc);
        Assert.Equal(scheduled.AppointmentId, roundTripped.AppointmentId);
        Assert.Equal(scheduled.PatientId, roundTripped.PatientId);
        Assert.Equal(scheduled.DentistId, roundTripped.DentistId);
        Assert.Equal(scheduled.StartsAt, roundTripped.StartsAt);
        Assert.Equal(scheduled.EndsAt, roundTripped.EndsAt);
    }
}