using System.Text.Json.Serialization;
using Domain.Entities;
using Infrastructure.Gateway;

namespace Infrastructure;

/// <summary>
/// JSON serialization context for Infrastructure layer AOT compilation.
/// Contains types for external HTTP calls and cache serialization.
/// </summary>
[JsonSerializable(typeof(HealthResponse))]
[JsonSerializable(typeof(PaymentProcessorRequest))]
[JsonSerializable(typeof(ProcessorHealthStatus))]
public partial class InfraJsonSerializerContext : JsonSerializerContext;