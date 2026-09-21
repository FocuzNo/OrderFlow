using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderFlow.Payments.Infrastructure.Persistence;

namespace OrderFlow.Payments.Infrastructure.Messaging;

public sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    IOptions<KafkaOptions> options,
    ILogger<OutboxProcessor> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatch(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Outbox batch failed for Payments");
            }
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    private async Task ProcessBatch(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IKafkaPublisher>();
        await using var transaction = await databaseContext.Database.BeginTransactionAsync(
            cancellationToken
        );
        var rows = await databaseContext
            .OutboxMessages.FromSqlRaw(
                "SELECT * FROM outbox_messages WHERE processed_on_utc IS NULL AND retry_count < {0} ORDER BY occurred_on_utc LIMIT {1} FOR UPDATE SKIP LOCKED",
                options.Value.MaxRetries,
                options.Value.OutboxBatchSize
            )
            .ToListAsync(cancellationToken);
        foreach (var row in rows)
        {
            try
            {
                await publisher.PublishAsync(
                    row.Type,
                    row.AggregateId,
                    row.Content,
                    cancellationToken
                );
                row.ProcessedOnUtc = DateTimeOffset.UtcNow;
                row.Error = null;
            }
            catch (Exception exception)
            {
                row.RetryCount++;
                row.Error =
                    exception.Message.Length > 2000 ? exception.Message[..2000] : exception.Message;
                logger.LogWarning(
                    exception,
                    "Failed to publish outbox message {MessageId} attempt {RetryCount}",
                    row.Id,
                    row.RetryCount
                );
            }
        }
        await databaseContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
