using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces;

public interface IProcessorHealthCache
{
    Task<ProcessorHealthStatus?> GetHealthAsync(
        PaymentProcessor processor,
        CancellationToken cancellationToken = default
    );

    Task SetHealthAsync(
        PaymentProcessor processor,
        ProcessorHealthStatus health,
        CancellationToken cancellationToken = default
    );
}