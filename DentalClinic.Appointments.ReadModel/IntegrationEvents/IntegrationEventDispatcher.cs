using System.Text.Json;
using DentalClinic.Appointments.Application.Abstractions.Messaging;
using DentalClinic.Appointments.Application.Features.Appointments.IntegrationEvents;

namespace DentalClinic.Appointments.ReadModel.IntegrationEvents;

public sealed class IntegrationEventDispatcher
{
    public IIntegrationEvent Deserialize(
        string eventType,
        string json)
    {
        var type = IntegrationEventTypeMap.GetTypeFor(eventType);

        var integrationEvent = JsonSerializer.Deserialize(json, type);

        return integrationEvent as IIntegrationEvent
            ?? throw new InvalidOperationException(
                $"The message for '{eventType}' did not represent an integration event.");
    }
}