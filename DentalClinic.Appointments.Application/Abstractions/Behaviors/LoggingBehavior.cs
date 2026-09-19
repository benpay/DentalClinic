using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DentalClinic.Appointments.Application.Abstractions.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling {RequestType} started: {@Request}",
            typeof(TRequest).Name,
            request);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();

            stopwatch.Stop();

            _logger.LogInformation(
                "Handling {RequestType} completed in {ElapsedMilliseconds} ms.",
                typeof(TRequest).Name,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();

            _logger.LogError(
                exception,
                "Handling {RequestType} failed in {ElapsedMilliseconds} ms.",
                typeof(TRequest).Name,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}