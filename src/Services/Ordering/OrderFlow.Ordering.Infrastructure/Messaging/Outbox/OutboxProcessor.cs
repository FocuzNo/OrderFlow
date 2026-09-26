using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderFlow.Ordering.Infrastructure.Messaging.Kafka;
using OrderFlow.Ordering.Infrastructure.Persistence;

namespace OrderFlow.Ordering.Infrastructure.Messaging.Outbox;

public sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    IKafkaPublisher kafkaPublisher,
    IOptions<OutboxOptions> options,
    ILogger<OutboxProcessor> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken
    )
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(
                    stoppingToken
                );
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Unexpected error while processing outbox messages"
                );
            }

            await Task.Delay(
                TimeSpan.FromMilliseconds(
                    options.Value.PollingIntervalMilliseconds
                ),
                stoppingToken
            );
        }
    }

    private async Task ProcessBatchAsync(
        CancellationToken cancellationToken
    )
    {
        await using var scope =
            scopeFactory.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<OrderingDbContext>();

        var outboxOptions =
            options.Value;

        var messages = await dbContext
            .OutboxMessages
            .Where(message =>
                message.ProcessedAt == null
                && message.RetryCount < outboxOptions.MaxRetries
            )
            .OrderBy(message =>
                message.OccurredAt
            )
            .Take(
                outboxOptions.BatchSize
            )
            .ToListAsync(
                cancellationToken
            );

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

                message.MarkAsProcessed(
                    DateTimeOffset.UtcNow
                );

                logger.LogInformation(
                    "Processed outbox message {OutboxMessageId} of type {OutboxMessageType}",
                    message.Id,
                    message.Type
                );
            }
            catch (Exception exception)
            {
                message.MarkAsFailed(
                    exception.Message
                );

                logger.LogWarning(
                    exception,
                    "Failed to publish outbox message {OutboxMessageId}. Retry {RetryCount}/{MaxRetries}",
                    message.Id,
                    message.RetryCount,
                    outboxOptions.MaxRetries
                );

                break;
            }
        }

        await dbContext.SaveChangesAsync(
            cancellationToken
        );
    }
}
