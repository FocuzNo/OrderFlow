using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderFlow.IntegrationEvents.Orders;
using OrderFlow.Inventory.Application.Inventory;

namespace OrderFlow.Inventory.Infrastructure.Messaging.Kafka;

public sealed class OrderCreatedKafkaConsumer(
    IOptions<KafkaOptions> options,
    IServiceScopeFactory scopeFactory,
    IKafkaPublisher kafkaPublisher,
    ILogger<OrderCreatedKafkaConsumer> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConsumeAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                // Rejoin from committed offsets. Never consume past an uncommitted failure.
                logger.LogError(exception, "Inventory consumer disconnected; retrying from committed offsets");
                await Task.Delay(options.Value.RetryDelayMilliseconds, stoppingToken);
            }
        }
    }

    private async Task ConsumeAsync(CancellationToken cancellationToken)
    {
        var kafkaOptions = options.Value;
        using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = kafkaOptions.BootstrapServers,
            GroupId = kafkaOptions.ConsumerGroup,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false,
            MaxPollIntervalMs = kafkaOptions.MaxPollIntervalMilliseconds,
        }).Build();

        consumer.Subscribe(kafkaOptions.OrderCreatedTopic);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var consumedMessage = consumer.Consume(cancellationToken);
                await ProcessWithRetryAsync(consumedMessage, cancellationToken);

                // Processing either committed Inbox + business state + Outbox, or Kafka acknowledged DLT.
                // A commit failure reconnects; Inbox protects redelivery after a successful DB commit.
                consumer.Commit(consumedMessage);
            }
        }
        finally
        {
            consumer.Close();
        }
    }

    public async Task ProcessWithRetryAsync(
        ConsumeResult<string, string> consumedMessage,
        CancellationToken cancellationToken
    )
    {
        var kafkaOptions = options.Value;

        for (var attempt = 0; ; attempt++)
        {
            try
            {
                await ProcessAsync(consumedMessage, cancellationToken);
                return;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogWarning(
                    exception,
                    "Inventory processing failed for {Topic}/{Partition}/{Offset}, attempt {Attempt}",
                    consumedMessage.Topic,
                    consumedMessage.Partition.Value,
                    consumedMessage.Offset.Value,
                    attempt + 1
                );

                if (attempt >= kafkaOptions.ProcessingMaxRetries)
                {
                    var headers = new Headers();
                    if (consumedMessage.Message.Headers is not null)
                    {
                        foreach (var header in consumedMessage.Message.Headers)
                            headers.Add(header.Key, header.GetValueBytes());
                    }

                    headers.Add("original-topic", Encoding.UTF8.GetBytes(consumedMessage.Topic));
                    headers.Add("original-partition", Encoding.UTF8.GetBytes(consumedMessage.Partition.Value.ToString()));
                    headers.Add("original-offset", Encoding.UTF8.GetBytes(consumedMessage.Offset.Value.ToString()));
                    headers.Add("error-type", Encoding.UTF8.GetBytes(exception.GetType().Name));

                    // Keep original key and payload. If DLT fails, propagate: no source offset commit.
                    await kafkaPublisher.PublishAsync(
                        kafkaOptions.OrderCreatedDeadLetterTopic,
                        consumedMessage.Message.Key,
                        consumedMessage.Message.Value,
                        cancellationToken,
                        headers
                    );
                    return;
                }

                await Task.Delay(kafkaOptions.RetryDelayMilliseconds, cancellationToken);
            }
        }
    }

    private async Task ProcessAsync(
        ConsumeResult<string, string> consumedMessage,
        CancellationToken cancellationToken
    )
    {
        var integrationEvent = JsonSerializer.Deserialize<OrderCreatedIntegrationEvent>(
            consumedMessage.Message.Value ?? throw new JsonException("OrderCreated payload is null.")
        ) ?? throw new JsonException("OrderCreated payload is null.");

        if (integrationEvent.Items is null || integrationEvent.Items.Any(item => item is null))
            throw new JsonException("OrderCreated items must not contain null values.");

        await using var scope = scopeFactory.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        await sender.Send(
            new ReserveOrderInventoryCommand(
                integrationEvent.EventId,
                integrationEvent.OrderId,
                integrationEvent.Items.Select(item =>
                    new ReserveOrderInventoryItem(item.ProductId, item.Quantity)
                ).ToArray()
            ),
            cancellationToken
        );
    }
}
