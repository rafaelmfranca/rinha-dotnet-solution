using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Domain.ValueObjects;
using Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace Infrastructure.Gateway;

public readonly record struct HealthResponse(
    [property: JsonPropertyName("failing")]
    bool Failing,
    [property: JsonPropertyName("minResponseTime")]
    int MinResponseTime);

public readonly record struct PaymentProcessorRequest(Guid CorrelationId, decimal Amount, DateTime RequestedAt);

public sealed class PaymentProcessorGateway(
    HttpClient httpClient,
    IOptions<PaymentProcessorSettings> settings) : IPaymentProcessorGateway
{
    public async Task<ProcessorHealthStatus> GetHealthAsync(
        PaymentProcessor processor,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = GetBaseUrl(processor);
            var response = await httpClient.GetAsync(
                $"{baseUrl}/payments/service-health",
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var health = await response.Content.ReadFromJsonAsync(
                    InfraJsonSerializerContext.Default.HealthResponse,
                    cancellationToken);

                return new ProcessorHealthStatus(!health.Failing, health.MinResponseTime);
            }
        }
        catch
        {
            // On any error, consider the processor unhealthy
        }

        return new ProcessorHealthStatus(false, int.MaxValue);
    }

    public async Task<PaymentResult> ProcessPaymentAsync(
        PaymentId correlationId,
        Money amount,
        DateTime requestedAt,
        PaymentProcessor processor,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = GetBaseUrl(processor);
            var request = new PaymentProcessorRequest(correlationId.Value, amount.Amount, requestedAt);

            using var content = new StringContent(
                JsonSerializer.Serialize(request, InfraJsonSerializerContext.Default.PaymentProcessorRequest),
                Encoding.UTF8,
                MediaTypeNames.Application.Json);

            var response = await httpClient.PostAsync($"{baseUrl}/payments", content, cancellationToken);
            if (response.IsSuccessStatusCode)
                return new PaymentResult(true);

            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            return new PaymentResult(false, error);
        }
        catch (Exception ex)
        {
            return new PaymentResult(false, ex.Message);
        }
    }

    private string GetBaseUrl(PaymentProcessor processor)
        => processor switch
        {
            PaymentProcessor.Default => settings.Value.DefaultBaseUrl,
            PaymentProcessor.Fallback => settings.Value.FallbackBaseUrl,
            _ => throw new ArgumentOutOfRangeException(nameof(processor))
        };
}