using System.Text.Json.Serialization;
using Api.Models;
using Domain.Entities;

namespace Api;

/// <summary>
/// JSON serialization context for API layer AOT compilation.
/// Contains types for HTTP request/response serialization.
/// </summary>
[JsonSerializable(typeof(PaymentApiRequest))]
[JsonSerializable(typeof(PaymentSummaryApiResponse))]
[JsonSerializable(typeof(ProcessorSummaryApi))]
[JsonSerializable(typeof(ProcessorHealthStatus))]
[JsonSerializable(typeof(HealthStatusResponse))]
[JsonSerializable(typeof(IEnumerable<KeyValuePair<string, string>>))]
public partial class ApiJsonSerializerContext : JsonSerializerContext;