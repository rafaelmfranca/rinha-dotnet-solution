using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public sealed class PaymentProcessorSelector(IProcessorHealthCache healthCache)
{
    private const int MaxResponseTimeMs = 30;

    public async Task<PaymentProcessor> ChooseBestProcessorAsync(CancellationToken cancellationToken = default)
    {
        var defaultHealth = await healthCache.GetHealthAsync(PaymentProcessor.Default, cancellationToken);
        var fallbackHealth = await healthCache.GetHealthAsync(PaymentProcessor.Fallback, cancellationToken);

        var defaultScore = GetProcessorScore(defaultHealth);
        var fallbackScore = GetProcessorScore(fallbackHealth);

        if (defaultScore == 0 && fallbackScore == 0)
            return PaymentProcessor.None;

        if (defaultScore > fallbackScore)
            return PaymentProcessor.Default;

        if (fallbackScore > defaultScore)
            return PaymentProcessor.Fallback;

        return defaultHealth?.MinResponseTimeMs <= fallbackHealth?.MinResponseTimeMs
            ? PaymentProcessor.Default
            : PaymentProcessor.Fallback;
    }

    private static int GetProcessorScore(ProcessorHealthStatus? healthStatus)
        => healthStatus switch
        {
            null or { IsHealthy: false } => 0,
            { IsHealthy: true, MinResponseTimeMs: <= MaxResponseTimeMs } => 2,
            { IsHealthy: true } => 1,
        };
}