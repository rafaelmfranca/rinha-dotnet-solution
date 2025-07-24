using Api.Models;
using Application.Contracts;
using Application.Services;

namespace Api;

public static class Endpoints
{
    public static void MapApiEndpoints(this WebApplication app)
    {
        app.MapPost("/payments", async (
            PaymentApiRequest request,
            PaymentService paymentService,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                var paymentRequest = new PaymentRequest(request.CorrelationId, request.Amount);
                _ = await paymentService.SubmitPaymentAsync(paymentRequest, cancellationToken);
                return Results.Accepted();
            }
            catch (Exception)
            {
                return Results.InternalServerError();
            }
        });

        app.MapGet("/payments-summary", async (
            PaymentService paymentService,
            DateTime? from = null,
            DateTime? to = null,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                var summary = await paymentService.GetSummaryAsync(from, to, cancellationToken);
                var response = new PaymentSummaryApiResponse(
                    new ProcessorSummaryApi(summary.Default.TotalRequests, summary.Default.TotalAmount),
                    new ProcessorSummaryApi(summary.Fallback.TotalRequests, summary.Fallback.TotalAmount)
                );
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                return Results.InternalServerError(ex.Message);
            }
        });

        app.MapPost("/purge-payments", async (
            PaymentService paymentService,
            CancellationToken cancellationToken = default) =>
        {
            await paymentService.PurgePaymentsAsync(cancellationToken);
            return Results.NoContent();
        });

        app.MapGet("/health", () => Results.Ok(new HealthStatusResponse("healthy", DateTime.UtcNow)));

        app.MapGet("/config", (IConfiguration configuration) => Results.Ok(configuration.AsEnumerable()));
    }
}