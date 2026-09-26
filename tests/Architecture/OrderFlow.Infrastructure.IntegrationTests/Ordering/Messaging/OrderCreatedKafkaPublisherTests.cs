using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OrderFlow.IntegrationEvents.Orders;
using OrderFlow.Ordering.Domain.Orders;
using OrderFlow.Ordering.Infrastructure.Messaging.Kafka;
using OrderFlow.Ordering.Infrastructure.Messaging.Kafka.Producers;
using Testcontainers.Kafka;

namespace OrderFlow.Infrastructure.IntegrationTests.Ordering.Messaging;

public sealed class OrderCreatedKafkaPublisherTests
{
    [Fact]
    public async Task PublishAsync_ShouldPublishOrderCreatedEvent()
    {
        await using var kafkaContainer = new KafkaBuilder("confluentinc/cp-kafka:7.5.12")
            .WithKRaft()
            .Build();

        await kafkaContainer.StartAsync();

        var bootstrapServers =
            kafkaContainer.GetBootstrapAddress();

        const string topic = "orderflow.order.created.test";

        using var adminClient =
            new AdminClientBuilder(
                new AdminClientConfig
                {
                    BootstrapServers = bootstrapServers,
                }
            )
            .Build();

        await adminClient.CreateTopicsAsync(
            [
                new TopicSpecification
                {
                    Name = topic,
                    NumPartitions = 3,
                    ReplicationFactor = 1,
                },
            ]
        );

        using var producer =
            new ProducerBuilder<string, string>(
                new ProducerConfig
                {
                    BootstrapServers = bootstrapServers,
                    ClientId = "orderflow-ordering-integration-tests",
                    Acks = Acks.All,
                    EnableIdempotence = true,
                }
            )
            .Build();

        var kafkaOptions = Options.Create(
            new KafkaOptions
            {
                BootstrapServers = bootstrapServers,
                OrderCreatedTopic = topic,
            }
        );

        var publisher = new OrderCreatedKafkaPublisher(
            producer,
            kafkaOptions,
            NullLogger<OrderCreatedKafkaPublisher>.Instance
        );

        var productId = Guid.NewGuid();

        var shippingAddress = ShippingAddress.Create(
            "15 Main Street",
            "Warsaw",
            "00-001",
            "PL"
        );

        var orderItem = OrderItem.Create(
            productId,
            "Mechanical Keyboard",
            129.99m,
            2
        );

        var order = Order.Create(
            Guid.NewGuid(),
            "buyer@example.com",
            shippingAddress,
            [orderItem]
        );

        await publisher.PublishAsync(
            order,
            CancellationToken.None
        );

        using var consumer =
            new ConsumerBuilder<string, string>(
                new ConsumerConfig
                {
                    BootstrapServers = bootstrapServers,
                    GroupId =
                        $"order-created-publisher-test-{Guid.NewGuid()}",
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    EnableAutoCommit = false,
                }
            )
            .Build();

        consumer.Subscribe(topic);

        var consumeResult = consumer.Consume(
            TimeSpan.FromSeconds(10)
        );

        Assert.NotNull(consumeResult);

        Assert.Equal(
            order.Id.ToString(),
            consumeResult.Message.Key);

        var integrationEvent =
            JsonSerializer.Deserialize<OrderCreatedIntegrationEvent>(
                consumeResult.Message.Value);

        Assert.NotNull(integrationEvent);

        Assert.NotEqual(
            Guid.Empty,
            integrationEvent.EventId);

        Assert.Equal(
            order.Id,
            integrationEvent.OrderId);

        Assert.Equal(
            order.CreatedAt,
            integrationEvent.OccurredAt);

        var publishedItem =
            Assert.Single(integrationEvent.Items);

        Assert.Equal(
            productId,
            publishedItem.ProductId);

        Assert.Equal(
            2,
            publishedItem.Quantity);
    }
}
