using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public sealed record PaymentApiRequest([Required] Guid CorrelationId, [Required] decimal Amount);

public sealed record PaymentSummaryApiResponse(ProcessorSummaryApi Default, ProcessorSummaryApi Fallback);

public sealed record ProcessorSummaryApi(int TotalRequests, decimal TotalAmount);