using DentalClinic.Appointments.ReadModel.Appointments;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DentalClinic.Appointments.ReadModel.DependencyInjection;

/// <summary>
/// Ensures the read model schema exists when the host starts, so that read
/// queries do not depend on the worker running first.
/// </summary>
public sealed class ReadModelInitializer : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReadModelInitializer> _logger;

    public ReadModelInitializer(
        IServiceScopeFactory scopeFactory,
        ILogger<ReadModelInitializer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ReadAppointmentsDbContext>();

        await ReadModelSchema.EnsureCreatedAsync(
            dbContext,
            _logger,
            cancellationToken);

        _logger.LogInformation(
            "Read model schema ensured at startup.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}