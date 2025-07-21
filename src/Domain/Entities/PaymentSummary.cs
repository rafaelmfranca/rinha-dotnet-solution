using Domain.ValueObjects;

namespace Domain.Entities;

public readonly record struct PaymentSummary(int TotalRequests, Money TotalAmount);