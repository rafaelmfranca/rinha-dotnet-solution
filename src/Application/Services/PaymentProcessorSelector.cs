using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public sealed class PaymentProcessorSelector(IProcessorHealthCache healthCache)
{
    // private PaymentProcessor _cachedSelection = PaymentProcessor.Default;
    // private DateTime _lastSelectionTime = DateTime.MinValue;
    // private readonly TimeSpan _selectionCacheInterval = TimeSpan.FromSeconds(1);

    public async Task<PaymentProcessor> ChooseBestProcessorAsync(CancellationToken cancellationToken = default)
    {
        // if (DateTime.UtcNow - _lastSelectionTime < _selectionCacheInterval)
        //     return _cachedSelection;

        var defaultHealth = await healthCache.GetHealthAsync(PaymentProcessor.Default, cancellationToken);
        var fallbackHealth = await healthCache.GetHealthAsync(PaymentProcessor.Fallback, cancellationToken);

        return EvaluateProcessorChoice(defaultHealth, fallbackHealth);
        // _lastSelectionTime = DateTime.UtcNow;
        // return _cachedSelection;
    }

    private static PaymentProcessor EvaluateProcessorChoice(
        ProcessorHealthStatus defaultHealth,
        ProcessorHealthStatus fallbackHealth)
    {
        if (!defaultHealth.IsHealthy && !fallbackHealth.IsHealthy)
            return PaymentProcessor.None;

        if (defaultHealth.IsHealthy && !fallbackHealth.IsHealthy)
            return PaymentProcessor.Default;

        if (!defaultHealth.IsHealthy && fallbackHealth.IsHealthy)
            return PaymentProcessor.Fallback;

        return defaultHealth.MinResponseTimeMs > fallbackHealth.MinResponseTimeMs * 2
            ? PaymentProcessor.Fallback
            : PaymentProcessor.Default;
    }
}