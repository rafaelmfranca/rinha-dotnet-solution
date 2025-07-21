namespace Infrastructure.Configuration;

public sealed class CacheSettings
{
    public const string SectionName = "Cache";

    public string ConnectionString { get; init; } = "cache:6379";
}