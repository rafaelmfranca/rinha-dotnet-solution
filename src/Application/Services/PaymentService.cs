using Application.Contracts;
using Application.Interfaces;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Services;

public sealed class PaymentService(IPaymentQueue paymentQueue, IPaymentRepository paymentRepository)
{
    public async Task<PaymentResponse> SubmitPaymentAsync(
        PaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        await paymentQueue.EnqueueAsync(request, cancellationToken);
        return new PaymentResponse(IsAccepted: true);
    }

    public async Task PurgePaymentsAsync(CancellationToken cancellationToken = default)
        => await paymentRepository.PurgeAsync(cancellationToken);

    public async Task<PaymentSummaryResponse> GetSummaryAsync(
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var summaries = await paymentRepository.GetSummaryByProcessorAsync(from, to, cancellationToken);

        var defaultSummary = summaries[PaymentProcessor.Default];
        var fallbackSummary = summaries[PaymentProcessor.Fallback];

        return new PaymentSummaryResponse(
            new ProcessorSummary(defaultSummary.TotalRequests, defaultSummary.TotalAmount),
            new ProcessorSummary(fallbackSummary.TotalRequests, fallbackSummary.TotalAmount)
        );
    }
}