using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderFlow.Inventory.Infrastructure.Messaging.Kafka;
using OrderFlow.Inventory.Infrastructure.Persistence;

namespace OrderFlow.Inventory.Infrastructure.Messaging.Outbox;

public sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    IKafkaPublisher kafkaPublisher,
    IOptions<OutboxOptions> options,
    ILogger<OutboxProcessor> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Outbox batch failed; uncommitted messages remain pending");
            }

            await Task.Delay(options.Value.PollingIntervalMilliseconds, stoppingToken);
        }
    }

    public async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var outboxOptions = options.Value;

        // Locks prevent concurrent workers from publishing the same rows in parallel.
        // A crash after Kafka acknowledgement may still redeliver: consumers must be idempotent.
        await using var transaction = await databaseContext.Database.BeginTransactionAsync(cancellationToken);
        var messages = await databaseContext.OutboxMessages
            .FromSqlInterpolated($"""
                SELECT * FROM outbox_messages
                WHERE processed_at IS NULL AND retry_count < {outboxOptions.MaxRetries}
                ORDER BY occurred_at, id
                LIMIT {outboxOptions.BatchSize}
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await kafkaPublisher.PublishAsync(
                    message.Topic,
                    message.Key,
                    message.Content,
                    cancellationToken
                );
                message.MarkAsProcessed(DateTimeOffset.UtcNow);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                message.MarkAsFailed(exception.Message);
                logger.LogWarning(
                    exception,
                    "Outbox {MessageId} failed: {RetryCount}/{MaxRetries}",
                    message.Id,
                    message.RetryCount,
                    outboxOptions.MaxRetries
                );
            }
        }

        await databaseContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
