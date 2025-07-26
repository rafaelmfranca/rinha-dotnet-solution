using Application.Contracts;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Workers;

public sealed class PaymentProcessorWorker(
    IPaymentQueue paymentQueue,
    IPaymentRepository paymentRepository,
    IPaymentProcessorGateway processorGateway,
    PaymentProcessorSelector processorSelector,
    ILogger<PaymentProcessorWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var request = await paymentQueue.DequeueAsync(cancellationToken);
                if (request.HasValue)
                    await HandlePaymentAsync(request.Value, cancellationToken);
                else
                    await Task.Delay(TimeSpan.FromMilliseconds(5), cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing payment queue.");
                await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
            }
        }
    }

    private async Task HandlePaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        const int maxImmediateRetries = 1;
        var requestedAt = DateTime.UtcNow;

        for (var attempt = 0; attempt <= maxImmediateRetries; attempt++)
        {
            try
            {
                var processor = await processorSelector.ChooseBestProcessorAsync(cancellationToken);
                if (processor is PaymentProcessor.None)
                {
                    await paymentQueue.EnqueueWithDelay(request, TimeSpan.FromSeconds(1), cancellationToken);
                    return;
                }

                var result = await processorGateway.ProcessPaymentAsync(
                    request.CorrelationId,
                    request.Amount,
                    requestedAt,
                    processor,
                    cancellationToken
                );

                if (!result.IsSuccess)
                    continue;

                var payment = Payment.Create(request.CorrelationId, request.Amount, requestedAt, processor);
                await paymentRepository.SaveAsync(payment, cancellationToken);

                if (processor is not PaymentProcessor.Fallback) 
                    return;
                
                var delay = await GetFallbackDelayAsync(cancellationToken);
                await Task.Delay(TimeSpan.FromMilliseconds(delay), cancellationToken);
                return;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Payment attempt {Attempt} failed.", attempt + 1);
            }
        }

        await paymentQueue.EnqueueWithDelay(request, TimeSpan.FromSeconds(1), cancellationToken);
    }

    private async Task<int> GetFallbackDelayAsync(CancellationToken cancellationToken = default) 
        => await paymentQueue.GetApproximateCountAsync(cancellationToken) switch
        {
            > 1000 => 30,
            _ => 60
        };
}