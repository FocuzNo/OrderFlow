using System.Text.Json;
using Confluent.Kafka;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OrderFlow.IntegrationEvents.Orders;
using OrderFlow.Inventory.Application.Inventory;
using OrderFlow.Inventory.Infrastructure.Messaging.Kafka;

namespace OrderFlow.Infrastructure.IntegrationTests;

public sealed class InventoryConsumerRetryTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Consumer_ShouldRetryTechnicalErrors_AndPropagateDeadLetterFailure(bool failDeadLetter)
    {
        var attempts = new Attempts();
        var services = new ServiceCollection();
        services.AddLogging();
        OrderFlow.Inventory.Application.DependencyInjection.AddApplication(services);
        services.AddSingleton(attempts);
        services.AddScoped<IRequestHandler<ReserveOrderInventoryCommand, ReserveOrderInventoryResponse>, FailingHandler>();
        await using var provider = services.BuildServiceProvider();
        var publisher = new RecordingPublisher { Fail = failDeadLetter };
        using var consumer = new OrderCreatedKafkaConsumer(
            Options.Create(new KafkaOptions {
                BootstrapServers = "unused:9092",
                ConsumerGroup = "test",
                OrderCreatedTopic = "created",
                OrderCreatedDeadLetterTopic = "dlt",
                InventoryReservedTopic = "reserved",
                InventoryReservationFailedTopic = "failed",
                ProcessingMaxRetries = 2,
                RetryDelayMilliseconds = 1,
            }),
            provider.GetRequiredService<IServiceScopeFactory>(),
            publisher,
            NullLogger<OrderCreatedKafkaConsumer>.Instance
        );
        var message = new ConsumeResult<string, string> {
            Topic = "created", Partition = 1, Offset = 42,
            Message = new() {
                Key = "order-key",
                Value = JsonSerializer.Serialize(new OrderCreatedIntegrationEvent(
                    Guid.NewGuid(), DateTimeOffset.UtcNow, Guid.NewGuid(), [new(Guid.NewGuid(), 1)]
                )),
                Headers = new Headers { { "correlation-id", new byte[] { 1, 2 } } }
            }
        };

        if (failDeadLetter)
            await Assert.ThrowsAsync<IOException>(() => consumer.ProcessWithRetryAsync(message, default));
        else
            await consumer.ProcessWithRetryAsync(message, default);

        Assert.Equal(3, attempts.Count);
        Assert.Equal("dlt", publisher.Topic);
        Assert.Equal(message.Message.Value, publisher.Content);
        Assert.Equal(message.Message.Key, publisher.Key);
        Assert.Contains(publisher.Headers!, header => header.Key == "correlation-id");
        // A DLT failure escapes processing, so ConsumeAsync cannot reach Commit.
    }

    public sealed class Attempts
    {
        public int Count { get; set; }
    }

    public sealed class FailingHandler(Attempts attempts)
        : IRequestHandler<ReserveOrderInventoryCommand, ReserveOrderInventoryResponse>
    {
        public Task<ReserveOrderInventoryResponse> Handle(
            ReserveOrderInventoryCommand request,
            CancellationToken cancellationToken
        )
        {
            attempts.Count++;
            throw new TimeoutException("Database unavailable");
        }
    }

    private sealed class RecordingPublisher : IKafkaPublisher
    {
        public bool Fail { get; init; }

        public string? Topic { get; private set; }

        public string? Key { get; private set; }

        public string? Content { get; private set; }

        public Headers? Headers { get; private set; }

        public Task PublishAsync(
            string topic,
            string key,
            string content,
            CancellationToken cancellationToken,
            Headers? headers = null
        )
        {
            Topic = topic;
            Key = key;
            Content = content;
            Headers = headers;
            return Fail ? Task.FromException(new IOException("Kafka unavailable")) : Task.CompletedTask;
        }
    }
}
