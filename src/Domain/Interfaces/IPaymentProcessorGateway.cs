using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Interfaces;

public interface IPaymentProcessorGateway
{
    Task<ProcessorHealthStatus> GetHealthAsync(
        PaymentProcessor processor,
        CancellationToken cancellationToken = default
    );

    Task<PaymentResult> ProcessPaymentAsync(
        PaymentId correlationId,
        Money amount,
        DateTime requestedAt,
        PaymentProcessor processor,
        CancellationToken cancellationToken = default
    );
}