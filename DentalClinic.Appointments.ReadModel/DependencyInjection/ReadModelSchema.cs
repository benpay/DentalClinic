using Npgsql;
using DentalClinic.Appointments.ReadModel.Appointments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DentalClinic.Appointments.ReadModel.DependencyInjection;

/// <summary>
/// Idempotent, race-tolerant schema creation for the read model database.
/// </summary>
public static class ReadModelSchema
{
    private const int MaxAttempts = 5;

    /// <summary>
    /// Calls <see cref="DatabaseFacade.EnsureCreatedAsync"/> treating a schema
    /// that already appeared (created concurrently by another process, e.g. the
    /// worker) as success. Transient connection failures are retried.
    /// </summary>
    public static async Task EnsureCreatedAsync(
        ReadAppointmentsDbContext dbContext,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var attempt = 0;

        while (true)
        {
            attempt++;

            try
            {
                await dbContext.Database.EnsureCreatedAsync(cancellationToken);

                return;
            }
            catch (PostgresException exception)
                when (exception.SqlState is "42P07" or "42710")
            {
                // 42P07 = duplicate_table, 42710 = duplicate_object.
                // Another process already created the schema concurrently.
                return;
            }
            catch (Exception) when (attempt < MaxAttempts)
            {
                logger.LogWarning(
                    "Could not ensure the read model schema (attempt {Attempt}). Retrying...",
                    attempt);

                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }
    }
}