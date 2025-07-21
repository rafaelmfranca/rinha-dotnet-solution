using Application.Interfaces;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Workers;

public sealed class HealthCheckWorker(
    IProcessorHealthCache healthCache,
    IPaymentProcessorGateway processorGateway,
    ILogger<HealthCheckWorker> logger) : BackgroundService
{
    private static readonly TimeSpan HealthCheckIntervalMs = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var isDefaultMonitored = Environment.GetEnvironmentVariable("MONITORED_PROCESSOR") == "default";
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (isDefaultMonitored)
                    await CheckAndCacheProcessorHealthAsync(PaymentProcessor.Default, cancellationToken);
                else
                    await CheckAndCacheProcessorHealthAsync(PaymentProcessor.Fallback, cancellationToken);
                await Task.Delay(HealthCheckIntervalMs, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Health check cycle failed.");
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }

    private async Task CheckAndCacheProcessorHealthAsync(PaymentProcessor processor,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var healthStatus = await processorGateway.GetHealthAsync(processor, cancellationToken);
            await healthCache.SetHealthAsync(processor, healthStatus, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Health check failed for {Processor}", processor);
        }
    }
}