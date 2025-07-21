using Domain.ValueObjects;

namespace Application.Contracts;

public readonly record struct PaymentRequest(PaymentId CorrelationId, Money Amount);