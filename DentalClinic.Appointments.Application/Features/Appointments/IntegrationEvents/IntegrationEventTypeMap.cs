namespace DentalClinic.Appointments.Application.Features.Appointments.IntegrationEvents;

/// <summary>
/// Maps integration event CLR types to stable, versioned wire names. The same
/// name is used in the Outbox, the broker 'EventType' property and the read
/// model dispatcher, so producers and consumers never depend on assembly-qualified names.
/// </summary>
public static class IntegrationEventTypeMap
{
    private static readonly IReadOnlyDictionary<Type, string> NameByType =
        new Dictionary<Type, string>
        {
            [typeof(AppointmentScheduledIntegrationEvent)] =
                IntegrationEventTypeName.AppointmentScheduled,
            [typeof(AppointmentRescheduledIntegrationEvent)] =
                IntegrationEventTypeName.AppointmentRescheduled,
            [typeof(AppointmentCancelledIntegrationEvent)] =
                IntegrationEventTypeName.AppointmentCancelled
        };

    private static readonly IReadOnlyDictionary<string, Type> TypeByName =
        NameByType.ToDictionary(pair => pair.Value, pair => pair.Key);

    public static string GetNameFor(Type eventType)
    {
        if (!NameByType.TryGetValue(eventType, out var name))
        {
            throw new NotSupportedException(
                $"The type '{eventType.Name}' is not a supported integration event.");
        }

        return name;
    }

    public static Type GetTypeFor(string eventTypeName)
    {
        if (!TypeByName.TryGetValue(eventTypeName, out var type))
        {
            throw new NotSupportedException(
                $"The event type '{eventTypeName}' is not supported.");
        }

        return type;
    }
}