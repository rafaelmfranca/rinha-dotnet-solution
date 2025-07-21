namespace Application.Contracts;

public readonly record struct PaymentSummaryResponse(ProcessorSummary Default, ProcessorSummary Fallback);