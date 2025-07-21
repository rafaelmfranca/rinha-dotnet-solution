using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Npgsql;

namespace Infrastructure.Persistence;

public sealed class PaymentRepository(NpgsqlDataSource dataSource) : IPaymentRepository
{
    public async Task SaveAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        const string sql = """
                           INSERT INTO payments (correlation_id, amount, requested_at, processor)
                           VALUES ($1, $2, $3, $4)
                           ON CONFLICT (correlation_id) DO NOTHING;
                           """;

        await using var command = dataSource.CreateCommand(sql);
        command.Parameters.Add(new NpgsqlParameter { Value = payment.CorrelationId.Value });
        command.Parameters.Add(new NpgsqlParameter { Value = payment.Amount.Amount });
        command.Parameters.Add(new NpgsqlParameter { Value = payment.RequestedAt });
        command.Parameters.Add(new NpgsqlParameter { Value = (short)payment.Processor });
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task PurgeAsync(CancellationToken cancellationToken = default)
    {
        await using var command = dataSource.CreateCommand();
        command.CommandText = "TRUNCATE TABLE payments RESTART IDENTITY;";
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<Dictionary<PaymentProcessor, PaymentSummary>> GetSummaryByProcessorAsync(
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var response = new Dictionary<PaymentProcessor, PaymentSummary>
        {
            [PaymentProcessor.Default] = new(0, 0),
            [PaymentProcessor.Fallback] = new(0, 0)
        };

        var parameters = new List<NpgsqlParameter>();
        var sql = """
                  SELECT
                    processor,
                    COUNT(*),
                    COALESCE(SUM(amount), 0)
                  FROM payments
                  WHERE 1=1
                  """;

        if (from.HasValue)
        {
            sql += " AND requested_at >= $1";
            parameters.Add(new NpgsqlParameter { Value = from.Value });
        }

        if (to.HasValue)
        {
            sql += from.HasValue ? " AND requested_at <= $2" : " AND requested_at <= $1";
            parameters.Add(new NpgsqlParameter { Value = to.Value });
        }

        sql += " GROUP BY processor";

        await using var command = dataSource.CreateCommand(sql);
        foreach (var param in parameters)
            command.Parameters.Add(param);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var processor = (PaymentProcessor)reader.GetInt16(0);
            response[processor] = new PaymentSummary(reader.GetInt32(1), reader.GetDecimal(2));
        }

        return response;
    }
}