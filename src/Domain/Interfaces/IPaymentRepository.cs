using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces;

public interface IPaymentRepository
{
    Task SaveAsync(Payment payment, CancellationToken cancellationToken = default);

    Task PurgeAsync(CancellationToken cancellationToken = default);

    Task<Dictionary<PaymentProcessor, PaymentSummary>> GetSummaryByProcessorAsync(
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default
    );
}