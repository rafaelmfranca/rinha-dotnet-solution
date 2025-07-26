using System.Collections.Concurrent;
using Application.Contracts;
using Application.Interfaces;

namespace Infrastructure.Queue;

public sealed class InMemoryPaymentQueue : IPaymentQueue
{
    private readonly ConcurrentQueue<PaymentRequest> _immediateQueue = new();
    private readonly ConcurrentQueue<(PaymentRequest Request, DateTime AvailableAt)> _delayedQueue = new();

    public Task EnqueueAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        _immediateQueue.Enqueue(request);
        return Task.CompletedTask;
    }

    public Task EnqueueWithDelay(
        PaymentRequest request,
        TimeSpan delay,
        CancellationToken cancellationToken = default)
    {
        var availableAt = DateTime.UtcNow.Add(delay);
        _delayedQueue.Enqueue((request, availableAt));
        return Task.CompletedTask;
    }

    public Task<PaymentRequest?> DequeueAsync(CancellationToken cancellationToken = default)
    {
        while (_delayedQueue.TryPeek(out var delayed) && DateTime.UtcNow >= delayed.AvailableAt)
        {
            if (_delayedQueue.TryDequeue(out delayed))
                return Task.FromResult<PaymentRequest?>(delayed.Request);
        }

        return _immediateQueue.TryDequeue(out var request)
            ? Task.FromResult<PaymentRequest?>(request)
            : Task.FromResult<PaymentRequest?>(null);
    }

    public Task<int> GetApproximateCountAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_immediateQueue.Count + _delayedQueue.Count);
}