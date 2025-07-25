using Application.Interfaces;
using Domain.Interfaces;
using Infrastructure.Cache;
using Infrastructure.Configuration;
using Infrastructure.Gateway;
using Infrastructure.Persistence;
using Infrastructure.Queue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using StackExchange.Redis;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        => services
            .ConfigureOptions(configuration)
            .AddNpgsql()
            .AddRedis()
            .AddSingleton<IPaymentQueue, InMemoryPaymentQueue>()
            .AddSingleton<IPaymentRepository, PaymentRepository>()
            .AddSingleton<IProcessorHealthCache, ProcessorHealthCache>()
            .AddHttpClient<IPaymentProcessorGateway, PaymentProcessorGateway>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(3);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                MaxConnectionsPerServer = 20,
                EnableMultipleHttp2Connections = true
            })
            .Services;

    private static IServiceCollection ConfigureOptions(this IServiceCollection services, IConfiguration configuration)
        => services
            .Configure<CacheSettings>(configuration.GetSection(CacheSettings.SectionName))
            .Configure<DatabaseSettings>(configuration.GetSection(DatabaseSettings.SectionName))
            .Configure<PaymentProcessorSettings>(configuration.GetSection(PaymentProcessorSettings.SectionName));

    private static IServiceCollection AddNpgsql(this IServiceCollection services)
        => services.AddSingleton<NpgsqlDataSource>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<DatabaseSettings>>();
            return NpgsqlDataSource.Create(settings.Value.ConnectionString);
        });

    private static IServiceCollection AddRedis(this IServiceCollection services)
        => services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<CacheSettings>>();
            return ConnectionMultiplexer.Connect(settings.Value.ConnectionString);
        });
}