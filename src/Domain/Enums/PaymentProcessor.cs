namespace Domain.Enums;

public enum PaymentProcessor
{
    Default = 0,
    Fallback = 1,
    None = 2
}

public static class PaymentProcessorExtensions
{
    public static string ToServiceName(this PaymentProcessor processor)
        => processor switch
        {
            PaymentProcessor.Default => "default",
            PaymentProcessor.Fallback => "fallback",
            PaymentProcessor.None => "none",
            _ => throw new ArgumentOutOfRangeException(nameof(processor))
        };
}