using System.Text.Json;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using StackExchange.Redis;

namespace Infrastructure.Cache;

public sealed class ProcessorHealthCache(IDatabase database) : IProcessorHealthCache
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(10);
    private const string KeyPrefix = "processor_health:";

    public async Task<ProcessorHealthStatus> GetHealthAsync(
        PaymentProcessor processor,
        CancellationToken cancellationToken = default)
    {
        var value = await database.StringGetAsync(new RedisKey(GetProcessorKey(processor)));
        return value.HasValue
            ? JsonSerializer.Deserialize(value.ToString(), InfraJsonSerializerContext.Default.ProcessorHealthStatus)
            : new ProcessorHealthStatus(false, int.MaxValue);
    }

    public async Task SetHealthAsync(
        PaymentProcessor processor,
        ProcessorHealthStatus health,
        CancellationToken cancellationToken = default)
    {
        var value = JsonSerializer.Serialize(health, InfraJsonSerializerContext.Default.ProcessorHealthStatus);
        await database.StringSetAsync(new RedisKey(GetProcessorKey(processor)), new RedisValue(value), CacheTtl);
    }

    private static string GetProcessorKey(PaymentProcessor processor)
        => KeyPrefix + processor.ToServiceName();
}