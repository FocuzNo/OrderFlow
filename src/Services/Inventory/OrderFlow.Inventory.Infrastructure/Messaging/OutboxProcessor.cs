using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderFlow.Inventory.Infrastructure.Persistence;

namespace OrderFlow.Inventory.Infrastructure.Messaging;

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
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox batch failed for Inventory");
            }
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    private async Task ProcessBatch(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IKafkaPublisher>();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var rows = await db
            .OutboxMessages.FromSqlRaw(
                "SELECT * FROM outbox_messages WHERE processed_on_utc IS NULL AND retry_count < {0} ORDER BY occurred_on_utc LIMIT {1} FOR UPDATE SKIP LOCKED",
                options.Value.MaxRetries,
                options.Value.OutboxBatchSize
            )
            .ToListAsync(ct);
        foreach (var row in rows)
        {
            try
            {
                await publisher.PublishAsync(row.Type, row.AggregateId, row.Content, ct);
                row.ProcessedOnUtc = DateTimeOffset.UtcNow;
                row.Error = null;
            }
            catch (Exception ex)
            {
                row.RetryCount++;
                row.Error = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
                logger.LogWarning(
                    ex,
                    "Failed to publish outbox message {MessageId} attempt {RetryCount}",
                    row.Id,
                    row.RetryCount
                );
            }
        }
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
}
