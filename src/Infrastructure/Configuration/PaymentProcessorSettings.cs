namespace Infrastructure.Configuration;

public sealed class PaymentProcessorSettings
{
    public const string SectionName = "PaymentProcessor";

    public string DefaultBaseUrl { get; init; } = "http://payment-processor-default:8080";
    public string FallbackBaseUrl { get; init; } = "http://payment-processor-fallback:8080";
}