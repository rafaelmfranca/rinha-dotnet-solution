using Application.Contracts;

namespace Application.Interfaces;

public interface IPaymentQueue
{
    Task EnqueueAsync(PaymentRequest request, CancellationToken cancellationToken = default);
    Task EnqueueWithDelay(PaymentRequest request, TimeSpan delay, CancellationToken cancellationToken = default);
    Task<PaymentRequest?> DequeueAsync(CancellationToken cancellationToken = default);
    Task<int> GetApproximateCountAsync(CancellationToken cancellationToken = default);
}