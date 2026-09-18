using DentalClinic.Appointments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DentalClinic.Appointments.Infrastructure.Messaging;

public sealed class OutboxProcessor : BackgroundService
{
    private static readonly TimeSpan ProcessingInterval =
        TimeSpan.FromSeconds(5);

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<OutboxProcessor> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingMessagesAsync(stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "An error occurred while processing Outbox messages.");
            }

            await Task.Delay(ProcessingInterval, stoppingToken);
        }
    }

    private async Task ProcessPendingMessagesAsync(
        CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<AppointmentsDbContext>();

        var publisher = scope.ServiceProvider
            .GetRequiredService<AzureServiceBusOutboxPublisher>();

        var pendingMessages = await dbContext.OutboxMessages
            .Where(message => message.ProcessedOnUtc == null)
            .OrderBy(message => message.OccurredOnUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var message in pendingMessages)
        {
            try
            {
                await publisher.PublishAsync(message, cancellationToken);

                message.MarkAsProcessed(DateTimeOffset.UtcNow);
            }
            catch (Exception exception)
            {
                message.MarkAsFailed(exception.Message);

                _logger.LogError(
                    exception,
                    "Failed to publish Outbox message {OutboxMessageId}.",
                    message.Id);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}