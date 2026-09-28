using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OrderFlow.Ordering.Application.Orders;
using OrderFlow.Ordering.Infrastructure.Messaging.Kafka;
using OrderFlow.Ordering.Infrastructure.Messaging.Outbox;
using OrderFlow.Ordering.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace OrderFlow.Infrastructure.IntegrationTests;

public sealed class OutboxReliabilityTests
{
    [DockerFact]
    public async Task Ordering_ShouldRollbackOrderWhenOutboxInsertFails_AndPersistBoundedRetries()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:18-alpine").Build();
        await postgres.StartAsync();
        var services = new ServiceCollection();
        services.AddDbContext<OrderingDbContext>(options =>
            options.UseNpgsql(postgres.GetConnectionString()).UseSnakeCaseNamingConvention()
        );
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        await databaseContext.Database.MigrateAsync();
        var command = new OrderFeatures.CreateOrderCommand(
            Guid.NewGuid(), "buyer@example.test", new("Main", "Warsaw", "00-001", "PL"),
            [new(Guid.NewGuid(), "Keyboard", 10, 2)]
        );
        var handler = new OrderFeatures.CreateOrderCommandHandler(
            new OrderRepository(databaseContext),
            databaseContext,
            new OrderCreatedOutboxWriter(databaseContext, Options.Create(new KafkaOptions {
                BootstrapServers = "unused:9092",
                OrderCreatedTopic = new string('x', 301),
            }))
        );
        await Assert.ThrowsAsync<DbUpdateException>(() => handler.Handle(command, default));
        databaseContext.ChangeTracker.Clear();
        Assert.Equal(0, await databaseContext.Orders.CountAsync());
        Assert.Equal(0, await databaseContext.OutboxMessages.CountAsync());

        handler = new OrderFeatures.CreateOrderCommandHandler(
            new OrderRepository(databaseContext),
            databaseContext,
            new OrderCreatedOutboxWriter(databaseContext, Options.Create(new KafkaOptions {
                BootstrapServers = "unused:9092", OrderCreatedTopic = "created"
            }))
        );
        await handler.Handle(command, default);
        var publisher = new FailingPublisher();
        using var processor = new OutboxProcessor(
            provider.GetRequiredService<IServiceScopeFactory>(),
            publisher,
            Options.Create(new OutboxOptions { MaxRetries = 2 }),
            NullLogger<OutboxProcessor>.Instance
        );
        await processor.ProcessBatchAsync(default);
        await processor.ProcessBatchAsync(default);
        await processor.ProcessBatchAsync(default);
        databaseContext.ChangeTracker.Clear();
        var message = await databaseContext.OutboxMessages.SingleAsync();
        Assert.Equal(2, publisher.Calls);
        Assert.Equal(2, message.RetryCount);
        Assert.Null(message.ProcessedAt);
        Assert.Equal(2000, message.Error!.Length);
        Assert.Equal(1, await databaseContext.Orders.CountAsync());
    }

    [DockerFact]
    public async Task Inventory_ShouldRollbackStockReservationAndInbox_WhenOutboxInsertFails()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:18-alpine").Build();
        await postgres.StartAsync();
        var contextOptions = new DbContextOptionsBuilder<OrderFlow.Inventory.Infrastructure.Persistence.InventoryDbContext>()
            .UseNpgsql(postgres.GetConnectionString())
            .UseSnakeCaseNamingConvention()
            .Options;
        await using var databaseContext = new OrderFlow.Inventory.Infrastructure.Persistence.InventoryDbContext(contextOptions);
        await databaseContext.Database.MigrateAsync();
        var warehouse = OrderFlow.Inventory.Domain.Warehouses.Warehouse.Create("Main", "Warsaw");
        var stockItem = OrderFlow.Inventory.Domain.Stock.StockItem.Create(Guid.NewGuid(), warehouse.Id, "SKU");
        stockItem.UpdateQuantity(10);
        databaseContext.Warehouses.Add(warehouse);
        databaseContext.StockItems.Add(stockItem);
        await databaseContext.SaveChangesAsync();
        var writer = new OrderFlow.Inventory.Infrastructure.Messaging.Outbox.InventoryOutboxWriter(
            databaseContext,
            Options.Create(new OrderFlow.Inventory.Infrastructure.Messaging.Kafka.KafkaOptions {
                BootstrapServers = "unused:9092",
                ConsumerGroup = "test",
                OrderCreatedTopic = "created",
                OrderCreatedDeadLetterTopic = "dlt",
                InventoryReservedTopic = new string('x', 301),
                InventoryReservationFailedTopic = "failed",
            })
        );
        var handler = new OrderFlow.Inventory.Application.Inventory.InventoryFeatures.ReserveOrderInventoryCommandHandler(
            new OrderFlow.Inventory.Infrastructure.Persistence.InventoryRepository(databaseContext),
            new OrderFlow.Inventory.Infrastructure.Persistence.Repositories.InboxRepository(databaseContext),
            writer,
            databaseContext
        );
        await Assert.ThrowsAsync<DbUpdateException>(() => handler.Handle(
            new OrderFlow.Inventory.Application.Inventory.ReserveOrderInventoryCommand(
                Guid.NewGuid(), Guid.NewGuid(), [new(stockItem.ProductId, 2)]
            ),
            default
        ));
        databaseContext.ChangeTracker.Clear();
        Assert.Equal(0, (await databaseContext.StockItems.SingleAsync()).ReservedQuantity);
        Assert.Equal(0, await databaseContext.Reservations.CountAsync());
        Assert.Equal(0, await databaseContext.InboxMessages.CountAsync());
        Assert.Equal(0, await databaseContext.OutboxMessages.CountAsync());
    }

    private sealed class FailingPublisher : IKafkaPublisher
    {
        public int Calls { get; private set; }

        public Task PublishAsync(
            string topic,
            string key,
            string content,
            CancellationToken cancellationToken
        )
        {
            Calls++;
            throw new IOException(new string('x', 3000));
        }
    }
}
