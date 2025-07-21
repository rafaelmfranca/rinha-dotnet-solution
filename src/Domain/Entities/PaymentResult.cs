namespace Domain.Entities;

public readonly record struct PaymentResult(bool IsSuccess, string? ErrorMessage = null);