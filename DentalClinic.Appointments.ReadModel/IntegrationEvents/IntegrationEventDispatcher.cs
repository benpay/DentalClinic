using System.Text.Json;
using DentalClinic.Appointments.Application.Abstractions.Messaging;
using DentalClinic.Appointments.Application.Features.Appointments.IntegrationEvents;

namespace DentalClinic.Appointments.ReadModel.IntegrationEvents;

public sealed class IntegrationEventDispatcher
{
    private static readonly Dictionary<string, Type> EventTypes =
        new(StringComparer.Ordinal)
        {
            [typeof(AppointmentScheduledIntegrationEvent).FullName!] =
                typeof(AppointmentScheduledIntegrationEvent),
            [typeof(AppointmentRescheduledIntegrationEvent).FullName!] =
                typeof(AppointmentRescheduledIntegrationEvent),
            [typeof(AppointmentCancelledIntegrationEvent).FullName!] =
                typeof(AppointmentCancelledIntegrationEvent)
        };

    public IIntegrationEvent Deserialize(
        string eventType,
        string json)
    {
        var type = ResolveType(eventType);

        var integrationEvent = JsonSerializer.Deserialize(json, type);

        return integrationEvent as IIntegrationEvent
            ?? throw new InvalidOperationException(
                $"The message for '{eventType}' did not represent an integration event.");
    }

    private static Type ResolveType(string eventType)
    {
        var fullName = eventType.Split(',')[0].Trim();

        if (EventTypes.TryGetValue(fullName, out var type))
        {
            return type;
        }

        throw new NotSupportedException(
            $"The event type '{eventType}' is not supported.");
    }
}