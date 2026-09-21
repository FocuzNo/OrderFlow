using Microsoft.EntityFrameworkCore;
using OrderFlow.Inventory.Domain.Stock;
using OrderFlow.Inventory.Domain.Warehouses;
using Testcontainers.PostgreSql;
using CatalogContext = OrderFlow.Catalog.Infrastructure.Persistence.CatalogDbContext;
using InventoryContext = OrderFlow.Inventory.Infrastructure.Persistence.InventoryDbContext;
using NotificationsContext = OrderFlow.Notifications.Infrastructure.Persistence.NotificationsDbContext;
using OrderingAddress = OrderFlow.Ordering.Domain.Orders.ShippingAddress;
using OrderingAggregate = OrderFlow.Ordering.Domain.Orders.Order;
using OrderingContext = OrderFlow.Ordering.Infrastructure.Persistence.OrderingDbContext;
using PaymentsContext = OrderFlow.Payments.Infrastructure.Persistence.PaymentsDbContext;

namespace OrderFlow.Infrastructure.IntegrationTests;

public sealed class DockerFactAttribute : FactAttribute
{
    public DockerFactAttribute()
    {
        if (
            !string.Equals(
                Environment.GetEnvironmentVariable("RUN_DOCKER_TESTS"),
                "true",
                StringComparison.OrdinalIgnoreCase
            )
        )
            Skip = "Set RUN_DOCKER_TESTS=true and start Docker to run container integration tests.";
    }
}

public sealed class PostgresMigrationTests
{
    [DockerFact]
    public async Task Every_service_migration_applies_to_a_clean_PostgreSQL_database()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:18-alpine").Build();
        await postgres.StartAsync();

        await Apply<CatalogContext>(
            postgres.GetConnectionString(),
            options => new CatalogContext(options)
        );
        await Apply<InventoryContext>(
            postgres.GetConnectionString(),
            options => new InventoryContext(options)
        );
        await Apply<OrderingContext>(
            postgres.GetConnectionString(),
            options => new OrderingContext(options)
        );
        await Apply<PaymentsContext>(
            postgres.GetConnectionString(),
            options => new PaymentsContext(options)
        );
        await Apply<NotificationsContext>(
            postgres.GetConnectionString(),
            options => new NotificationsContext(options)
        );
    }

    [DockerFact]
    public async Task Inventory_enforces_inbox_idempotency_and_concurrent_reservations()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:18-alpine").Build();
        await postgres.StartAsync();
        var options = new DbContextOptionsBuilder<InventoryContext>()
            .UseNpgsql(postgres.GetConnectionString())
            .UseSnakeCaseNamingConvention()
            .Options;

        Guid stockId;
        await using (var setup = new InventoryContext(options))
        {
            await setup.Database.MigrateAsync();
            var warehouse = Warehouse.Create("Main", "Minsk");
            var stock = StockItem.Create(Guid.NewGuid(), warehouse.Id, "SKU-1");
            stock.Increase(5);
            setup.Warehouses.Add(warehouse);
            setup.StockItems.Add(stock);
            await setup.SaveChangesAsync();
            stockId = stock.Id;
        }

        await using var first = new InventoryContext(options);
        await using var second = new InventoryContext(options);
        var firstStock = await first
            .StockItems.Include(candidate => candidate.Reservations)
            .SingleAsync(candidate => candidate.Id == stockId);
        var secondStock = await second
            .StockItems.Include(candidate => candidate.Reservations)
            .SingleAsync(candidate => candidate.Id == stockId);
        firstStock.Reserve(Guid.NewGuid(), 4);
        secondStock.Reserve(Guid.NewGuid(), 4);
        await first.SaveChangesAsync();
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());

        var eventId = Guid.NewGuid();
        await using (var inbox = new InventoryContext(options))
        {
            inbox.InboxMessages.Add(
                new()
                {
                    Id = eventId,
                    Consumer = "test",
                    ProcessedOnUtc = DateTimeOffset.UtcNow,
                }
            );
            await inbox.SaveChangesAsync();
        }
        await using (var duplicate = new InventoryContext(options))
        {
            duplicate.InboxMessages.Add(
                new()
                {
                    Id = eventId,
                    Consumer = "test",
                    ProcessedOnUtc = DateTimeOffset.UtcNow,
                }
            );
            await Assert.ThrowsAsync<DbUpdateException>(() => duplicate.SaveChangesAsync());
        }
    }

    [DockerFact]
    public async Task Ordering_commits_aggregate_and_outbox_atomically()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:18-alpine").Build();
        await postgres.StartAsync();
        var options = new DbContextOptionsBuilder<OrderingContext>()
            .UseNpgsql(postgres.GetConnectionString())
            .UseSnakeCaseNamingConvention()
            .Options;
        await using var databaseContext = new OrderingContext(options);
        await databaseContext.Database.MigrateAsync();
        var order = OrderingAggregate.Create(
            Guid.NewGuid(),
            "buyer@example.test",
            OrderingAddress.Create("1 Main St", "Minsk", "220000", "BY")
        );
        order.AddItem(Guid.NewGuid(), "Notebook", 12.50m, 2);
        order.Submit();
        var repository = new OrderFlow.Ordering.Infrastructure.Persistence.OrderRepository(
            databaseContext
        );
        await repository.AddAsync(order, default);

        OrderFlow.Ordering.Application.Abstractions.Persistence.IUnitOfWork unitOfWork =
            databaseContext;
        await unitOfWork.SaveChangesAsync();

        Assert.Equal(1, await databaseContext.Orders.CountAsync());
        Assert.Equal(1, await databaseContext.OutboxMessages.CountAsync());
        var outboxMessage = await databaseContext.OutboxMessages.SingleAsync();
        var envelope =
            System.Text.Json.JsonSerializer.Deserialize<OrderFlow.IntegrationContracts.IntegrationEventEnvelope>(
                outboxMessage.Content
            )!;
        var integrationEvent =
            System.Text.Json.JsonSerializer.Deserialize<OrderFlow.IntegrationContracts.OrderSubmittedIntegrationEvent>(
                envelope.Payload
            )!;
        Assert.Equal(envelope.EventId, integrationEvent.EventId);
        Assert.Equal(envelope.OccurredOnUtc, integrationEvent.OccurredAt);
    }

    private static async Task Apply<TContext>(
        string connectionString,
        Func<DbContextOptions<TContext>, TContext> factory
    )
        where TContext : DbContext
    {
        var options = new DbContextOptionsBuilder<TContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;
        await using var context = factory(options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();
        Assert.True(await context.Database.CanConnectAsync());
        Assert.NotEmpty(await context.Database.GetAppliedMigrationsAsync());
    }
}
