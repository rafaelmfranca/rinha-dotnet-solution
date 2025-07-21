using Application.Services;
using Application.Workers;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
        => services
            .AddPaymentServices()
            .AddWorkers();

    private static IServiceCollection AddPaymentServices(this IServiceCollection services)
        => services
            .AddSingleton<PaymentService>()
            .AddSingleton<PaymentProcessorSelector>();

    private static IServiceCollection AddWorkers(this IServiceCollection services)
        => services
            .AddHostedService<PaymentProcessorWorker>()
            .AddHostedService<HealthCheckWorker>();
}