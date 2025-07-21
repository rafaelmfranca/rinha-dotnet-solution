using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Entities;

public sealed class Payment
{
    public PaymentId CorrelationId { get; private init; }
    public Money Amount { get; private init; }
    public DateTime RequestedAt { get; private init; }
    public PaymentProcessor Processor { get; private init; }

    public static Payment Create(
        PaymentId correlationId,
        Money amount,
        DateTime requestedAt,
        PaymentProcessor processor)
        => new()
        {
            CorrelationId = correlationId,
            Amount = amount,
            RequestedAt = requestedAt,
            Processor = processor,
        };
}