namespace Infrastructure.Configuration;

public sealed class DatabaseSettings
{
    public const string SectionName = "Database";

    public string ConnectionString { get; init; } = "Host=localhost;Database=rinha;Username=postgres;Password=postgres";
}