namespace Domain.Entities;

public readonly record struct ProcessorHealthStatus(bool IsHealthy, int MinResponseTimeMs);