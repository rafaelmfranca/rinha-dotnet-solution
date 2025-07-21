using System.Text.Json;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using StackExchange.Redis;

namespace Infrastructure.Cache;

public sealed class ProcessorHealthCache(IConnectionMultiplexer redis) : IProcessorHealthCache
{
    private readonly IDatabase _database = redis.GetDatabase();
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(10);

    private const string KeyPrefix = "processor_health:";

    public async Task<ProcessorHealthStatus?> GetHealthAsync(
        PaymentProcessor processor,
        CancellationToken cancellationToken = default)
    {
        var value = await _database.StringGetAsync(new RedisKey(GetProcessorKey(processor)));
        return value.HasValue
            ? JsonSerializer.Deserialize<ProcessorHealthStatus>(value.ToString())
            : null;
    }

    public async Task SetHealthAsync(
        PaymentProcessor processor,
        ProcessorHealthStatus health,
        CancellationToken cancellationToken = default)
    {
        var value = JsonSerializer.Serialize(health);
        await _database.StringSetAsync(new RedisKey(GetProcessorKey(processor)), new RedisValue(value), CacheTtl);
    }

    private static string GetProcessorKey(PaymentProcessor processor)
        => KeyPrefix + processor.ToServiceName();
}