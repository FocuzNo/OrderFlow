using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderFlow.IntegrationEvents.Orders;
using OrderFlow.Inventory.Application.Inventory;
using OrderFlow.Inventory.Domain.Stock;
using OrderFlow.Inventory.Domain.Warehouses;
using OrderFlow.Inventory.Infrastructure.Persistence;
using OrderFlow.Ordering.Application.Orders;
using OrderFlow.Ordering.Infrastructure.Persistence;
using Testcontainers.Kafka;
using Testcontainers.PostgreSql;
using InventoryConsumer = OrderFlow.Inventory.Infrastructure.Messaging.Kafka.OrderCreatedKafkaConsumer;
using InventoryProcessor = OrderFlow.Inventory.Infrastructure.Messaging.Outbox.OutboxProcessor;
using OrderingProcessor = OrderFlow.Ordering.Infrastructure.Messaging.Outbox.OutboxProcessor;

namespace OrderFlow.Infrastructure.IntegrationTests;

public sealed class OrderingInventoryKafkaTests
{
    [DockerFact]
    public async Task Flow_ShouldReserveOnce_PublishFailureWithoutPartialStock_AndDeadLetterInvalidJson()
    {
        await using var orderingDatabase = new PostgreSqlBuilder("postgres:18-alpine").Build();
        await using var inventoryDatabase = new PostgreSqlBuilder("postgres:18-alpine").Build();
        await using var kafka = new KafkaBuilder("confluentinc/cp-kafka:7.5.12").WithKRaft().Build();
        await Task.WhenAll(orderingDatabase.StartAsync(), inventoryDatabase.StartAsync(), kafka.StartAsync());
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        var cancellationToken = timeout.Token;
        var broker = kafka.GetBootstrapAddress();
        var topics = new[] {
            "orderflow.order.created", "orderflow.inventory.reserved",
            "orderflow.inventory.reservation-failed", "orderflow.inventory.order-created.dlt"
        };
        using var admin = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = broker }).Build();
        await admin.CreateTopicsAsync(topics.Select(topic => new TopicSpecification {
            Name = topic, NumPartitions = 3, ReplicationFactor = 1
        }));

        await using var ordering = BuildServices("Ordering", orderingDatabase.GetConnectionString(), broker);
        await using var inventory = BuildServices("Inventory", inventoryDatabase.GetConnectionString(), broker);
        await using var orderingScope = ordering.CreateAsyncScope();
        await using var inventoryScope = inventory.CreateAsyncScope();
        var orders = orderingScope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        var stock = inventoryScope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        await orders.Database.MigrateAsync(cancellationToken);
        await stock.Database.MigrateAsync(cancellationToken);
        Assert.Empty(await orders.Database.GetPendingMigrationsAsync(cancellationToken));
        Assert.Empty(await stock.Database.GetPendingMigrationsAsync(cancellationToken));

        var warehouse = Warehouse.Create("Kafka test", "Warsaw");
        var stockItem = StockItem.Create(Guid.NewGuid(), warehouse.Id, "KAFKA-TEST");
        stockItem.UpdateQuantity(10);
        stock.Warehouses.Add(warehouse);
        stock.StockItems.Add(stockItem);
        await stock.SaveChangesAsync(cancellationToken);

        var response = await orderingScope.ServiceProvider.GetRequiredService<ISender>().Send(
            new OrderFeatures.CreateOrderCommand(
                Guid.NewGuid(), "buyer@example.test", new("Main", "Warsaw", "00-001", "PL"),
                [new(stockItem.ProductId, "Keyboard", 10, 2)]
            ), cancellationToken
        );
        var originalMessage = await orders.OutboxMessages.AsNoTracking().SingleAsync(cancellationToken);
        Assert.Null(originalMessage.ProcessedAt);
        Assert.Equal(response.Id.ToString(), originalMessage.Key);

        var orderingProcessor = ordering.GetServices<IHostedService>().OfType<OrderingProcessor>().Single();
        await orderingProcessor.ProcessBatchAsync(cancellationToken);
        Assert.NotNull((await orders.OutboxMessages.AsNoTracking().SingleAsync(cancellationToken)).ProcessedAt);

        var inventoryConsumer = inventory.GetServices<IHostedService>().OfType<InventoryConsumer>().Single();
        await inventoryConsumer.StartAsync(cancellationToken);
        try
        {
            await WaitUntilAsync(() => stock.InboxMessages.AsNoTracking().AnyAsync(
                message => message.Id == originalMessage.Id, cancellationToken), cancellationToken);
            stock.ChangeTracker.Clear();
            Assert.Equal(2, (await stock.StockItems.SingleAsync(cancellationToken)).ReservedQuantity);
            Assert.Single(await stock.Reservations.ToListAsync(cancellationToken));
            Assert.Single(await stock.OutboxMessages.ToListAsync(cancellationToken));

            using var producer = new ProducerBuilder<string, string>(new ProducerConfig {
                BootstrapServers = broker, Acks = Acks.All, EnableIdempotence = true
            }).Build();
            // Exact redelivery of the persisted event, followed by another order in the same partition.
            await producer.ProduceAsync(topics[0], new() {
                Key = originalMessage.Key, Value = originalMessage.Content
            }, cancellationToken);
            var failedEvent = new OrderCreatedIntegrationEvent(
                Guid.NewGuid(), DateTimeOffset.UtcNow, Guid.NewGuid(),
                [new(stockItem.ProductId, 1), new(Guid.NewGuid(), 1)]
            );
            await producer.ProduceAsync(topics[0], new() {
                Key = originalMessage.Key, Value = JsonSerializer.Serialize(failedEvent)
            }, cancellationToken);
            await WaitUntilAsync(() => stock.InboxMessages.AsNoTracking().AnyAsync(
                message => message.Id == failedEvent.EventId, cancellationToken), cancellationToken);
            stock.ChangeTracker.Clear();
            Assert.Equal(2, (await stock.StockItems.SingleAsync(cancellationToken)).ReservedQuantity);
            Assert.Single(await stock.Reservations.ToListAsync(cancellationToken));
            Assert.Equal(2, await stock.InboxMessages.CountAsync(cancellationToken));
            Assert.Equal(2, await stock.OutboxMessages.CountAsync(cancellationToken));

            var inventoryProcessor = inventory.GetServices<IHostedService>().OfType<InventoryProcessor>().Single();
            await inventoryProcessor.ProcessBatchAsync(cancellationToken);
            Assert.All(await stock.OutboxMessages.AsNoTracking().ToListAsync(cancellationToken),
                message => Assert.NotNull(message.ProcessedAt));

            await producer.ProduceAsync(topics[0], new() {
                Key = originalMessage.Key, Value = "{malformed"
            }, cancellationToken);
            using var results = new ConsumerBuilder<string, string>(new ConsumerConfig {
                BootstrapServers = broker, GroupId = "assertions", AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            }).Build();
            results.Subscribe(topics.Skip(1));
            var received = new Dictionary<string, ConsumeResult<string, string>>();
            while (received.Count < 3)
            {
                var message = results.Consume(cancellationToken);
                received[message.Topic] = message;
            }
            Assert.Equal(response.Id.ToString(), received[topics[1]].Message.Key);
            Assert.Equal(failedEvent.OrderId.ToString(), received[topics[2]].Message.Key);
            Assert.Equal("{malformed", received[topics[3]].Message.Value);
            Assert.Equal(originalMessage.Key, received[topics[3]].Message.Key);
            Assert.Contains(received[topics[3]].Message.Headers, header => header.Key == "original-offset");
        }
        finally
        {
            await inventoryConsumer.StopAsync(CancellationToken.None);
        }
    }

    private static ServiceProvider BuildServices(string service, string connectionString, string broker)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> {
            [$"ConnectionStrings:{service}Database"] = connectionString,
            ["Kafka:BootstrapServers"] = broker,
            ["Kafka:OrderCreatedTopic"] = "orderflow.order.created",
            ["Kafka:ConsumerGroup"] = "orderflow-inventory",
            ["Kafka:InventoryReservedTopic"] = "orderflow.inventory.reserved",
            ["Kafka:InventoryReservationFailedTopic"] = "orderflow.inventory.reservation-failed",
            ["Kafka:OrderCreatedDeadLetterTopic"] = "orderflow.inventory.order-created.dlt",
            ["Kafka:RetryDelayMilliseconds"] = "10",
            ["Kafka:ProcessingMaxRetries"] = "2",
        }).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        if (service == "Ordering")
        {
            OrderFlow.Ordering.Application.DependencyInjection.AddApplication(services);
            OrderFlow.Ordering.Infrastructure.DependencyInjection.AddInfrastructure(services, configuration);
        }
        else
        {
            OrderFlow.Inventory.Application.DependencyInjection.AddApplication(services);
            OrderFlow.Inventory.Infrastructure.DependencyInjection.AddInfrastructure(services, configuration);
        }
        return services.BuildServiceProvider();
    }

    private static async Task WaitUntilAsync(Func<Task<bool>> condition, CancellationToken cancellationToken)
    {
        while (!await condition())
            await Task.Delay(100, cancellationToken);
    }
}
