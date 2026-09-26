using System.Text.Json;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderFlow.IntegrationEvents.Orders;
using OrderFlow.Inventory.Infrastructure.Persistence;
using OrderFlow.Inventory.Infrastructure.Persistence.Inbox;

namespace OrderFlow.Inventory.Infrastructure.Messaging.Kafka;

public sealed class OrderCreatedKafkaConsumer(
    IOptions<KafkaOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<OrderCreatedKafkaConsumer> logger
) : BackgroundService
{
    protected override Task ExecuteAsync(
        CancellationToken stoppingToken
    )
    {
        return Task.Run(
            () => Consume(
                stoppingToken
            ),
            stoppingToken
        );
    }

    private void Consume(
        CancellationToken cancellationToken
    )
    {
        var kafkaOptions =
            options.Value;

        var consumerConfig =
            new ConsumerConfig
            {
                BootstrapServers =
                    kafkaOptions.BootstrapServers,

                GroupId =
                    kafkaOptions.ConsumerGroup,

                AutoOffsetReset =
                    AutoOffsetReset.Earliest,

                EnableAutoCommit =
                    false,
            };

        using var consumer =
            new ConsumerBuilder<string, string>(
                consumerConfig
            )
            .Build();

        consumer.Subscribe(
            kafkaOptions.OrderCreatedTopic
        );

        logger.LogInformation(
            "Kafka consumer started. Topic: {Topic}, Group: {ConsumerGroup}",
            kafkaOptions.OrderCreatedTopic,
            kafkaOptions.ConsumerGroup
        );

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var consumeResult =
                    consumer.Consume(
                        cancellationToken
                    );

                try
                {
                    ProcessMessageAsync(
                            consumeResult,
                            cancellationToken
                        )
                        .GetAwaiter()
                        .GetResult();

                    consumer.Commit(
                        consumeResult
                    );

                    logger.LogInformation(
                        "Committed Kafka offset {Offset} for partition {Partition}",
                        consumeResult.Offset.Value,
                        consumeResult.Partition.Value
                    );
                }
                catch (JsonException exception)
                {
                    logger.LogError(
                        exception,
                        "Failed to deserialize OrderCreated event at partition {Partition}, offset {Offset}",
                        consumeResult.Partition.Value,
                        consumeResult.Offset.Value
                    );
                }
                catch (Exception exception)
                {
                    logger.LogError(
                        exception,
                        "Failed to process OrderCreated message at partition {Partition}, offset {Offset}",
                        consumeResult.Partition.Value,
                        consumeResult.Offset.Value
                    );
                }
            }
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation(
                "Kafka consumer is stopping"
            );
        }
        finally
        {
            consumer.Close();
        }
    }

    private async Task ProcessMessageAsync(
        ConsumeResult<string, string> consumeResult,
        CancellationToken cancellationToken
    )
    {
        var integrationEvent =
            JsonSerializer.Deserialize<OrderCreatedIntegrationEvent>(
                consumeResult.Message.Value
            );

        if (integrationEvent is null)
        {
            throw new JsonException(
                "OrderCreatedIntegrationEvent is null."
            );
        }

        await using var scope =
            scopeFactory.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<InventoryDbContext>();

        var alreadyProcessed =
            await dbContext.InboxMessages
                .AnyAsync(
                    message =>
                        message.Id == integrationEvent.EventId,
                    cancellationToken
                );

        if (alreadyProcessed)
        {
            logger.LogInformation(
                "Skipping duplicate OrderCreated event {EventId} for order {OrderId}",
                integrationEvent.EventId,
                integrationEvent.OrderId
            );

            return;
        }

        logger.LogInformation(
            "Processing OrderCreated event {EventId} for order {OrderId}. Partition: {Partition}, Offset: {Offset}",
            integrationEvent.EventId,
            integrationEvent.OrderId,
            consumeResult.Partition.Value,
            consumeResult.Offset.Value
        );

        foreach (var item in integrationEvent.Items)
        {
            logger.LogInformation(
                "Order {OrderId} contains product {ProductId}, quantity {Quantity}",
                integrationEvent.OrderId,
                item.ProductId,
                item.Quantity
            );
        }

        var inboxMessage =
            InboxMessage.Create(
                integrationEvent.EventId,
                nameof(OrderCreatedIntegrationEvent),
                DateTimeOffset.UtcNow
            );

        dbContext.InboxMessages.Add(
            inboxMessage
        );

        await dbContext.SaveChangesAsync(
            cancellationToken
        );

        logger.LogInformation(
            "Stored event {EventId} in Inbox",
            integrationEvent.EventId
        );
    }
}
