using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using CatalogPersistence = OrderFlow.Catalog.Infrastructure.Persistence;
using InventoryPersistence = OrderFlow.Inventory.Infrastructure.Persistence;
using NotificationsPersistence = OrderFlow.Notifications.Infrastructure.Persistence;
using OrderingPersistence = OrderFlow.Ordering.Infrastructure.Persistence;
using PaymentsPersistence = OrderFlow.Payments.Infrastructure.Persistence;

namespace OrderFlow.Infrastructure.IntegrationTests;

public sealed class PostgresMigrationTests
{
    private static DbContextOptions<TContext> Options<TContext>(string connectionString)
        where TContext : DbContext =>
        new DbContextOptionsBuilder<TContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

    [DockerFact]
    public async Task Catalog_ShouldPersistAndReadProduct()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:18-alpine").Build();
        await postgres.StartAsync();
        await using var context = new CatalogPersistence.CatalogDbContext(
            Options<CatalogPersistence.CatalogDbContext>(postgres.GetConnectionString())
        );
        await context.Database.MigrateAsync();
        var category = OrderFlow.Catalog.Domain.Categories.Category.Create("Office", null);
        context.Categories.Add(category);
        var product = OrderFlow.Catalog.Domain.Products.Product.Create(
            "SKU",
            "Notebook",
            null,
            12.5m,
            category.Id
        );
        var repository = new CatalogPersistence.Repositories.ProductRepository(context);
        await repository.AddAsync(product, default);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        Assert.Equal(12.5m, (await repository.GetByIdAsync(product.Id, default))!.Price.Amount);
    }

    [DockerFact]
    public async Task Ordering_ShouldPersistOrderWithItems()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:18-alpine").Build();
        await postgres.StartAsync();
        await using var context = new OrderingPersistence.OrderingDbContext(
            Options<OrderingPersistence.OrderingDbContext>(postgres.GetConnectionString())
        );
        await context.Database.MigrateAsync();
        var order = OrderFlow.Ordering.Domain.Orders.Order.Create(
            Guid.NewGuid(),
            "buyer@example.test",
            OrderFlow.Ordering.Domain.Orders.ShippingAddress.Create(
                "Main",
                "Minsk",
                "220000",
                "BY"
            ),
            [
                OrderFlow.Ordering.Domain.Orders.OrderItem.Create(
                    Guid.NewGuid(),
                    "Notebook",
                    12.5m,
                    2
                ),
            ]
        );
        var repository = new OrderingPersistence.OrderRepository(context);
        await repository.AddAsync(order, default);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var persisted = await repository.GetByIdAsync(order.Id, default);
        Assert.Single(persisted!.Items);
        Assert.Equal(25m, persisted.TotalAmount);
    }

    [DockerFact]
    public async Task Inventory_ShouldPersistReservationAndRejectDuplicateProduct()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:18-alpine").Build();
        await postgres.StartAsync();
        var options = Options<InventoryPersistence.InventoryDbContext>(
            postgres.GetConnectionString()
        );
        await using var context = new InventoryPersistence.InventoryDbContext(options);
        await context.Database.MigrateAsync();
        var warehouse = OrderFlow.Inventory.Domain.Warehouses.Warehouse.Create("Main", "Minsk");
        context.Warehouses.Add(warehouse);
        var stock = OrderFlow.Inventory.Domain.Stock.StockItem.Create(
            Guid.NewGuid(),
            warehouse.Id,
            "SKU"
        );
        stock.UpdateQuantity(10);
        stock.Reserve(Guid.NewGuid(), 4);
        context.StockItems.Add(stock);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var persisted = await context.StockItems.Include(item => item.Reservations).SingleAsync();
        Assert.Equal(6, persisted.AvailableQuantity);
        Assert.Single(persisted.Reservations);
        context.StockItems.Add(
            OrderFlow.Inventory.Domain.Stock.StockItem.Create(
                stock.ProductId,
                warehouse.Id,
                "OTHER"
            )
        );
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [DockerFact]
    public async Task Inventory_ShouldRejectConcurrentReservations()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:18-alpine").Build();
        await postgres.StartAsync();
        var options = Options<InventoryPersistence.InventoryDbContext>(
            postgres.GetConnectionString()
        );
        await using var setup = new InventoryPersistence.InventoryDbContext(options);
        await setup.Database.MigrateAsync();
        var warehouse = OrderFlow.Inventory.Domain.Warehouses.Warehouse.Create("Main", "Minsk");
        setup.Warehouses.Add(warehouse);
        var stock = OrderFlow.Inventory.Domain.Stock.StockItem.Create(
            Guid.NewGuid(),
            warehouse.Id,
            "SKU"
        );
        stock.UpdateQuantity(5);
        setup.StockItems.Add(stock);
        await setup.SaveChangesAsync();
        await using var first = new InventoryPersistence.InventoryDbContext(options);
        await using var second = new InventoryPersistence.InventoryDbContext(options);
        var firstStock = await first.StockItems.Include(item => item.Reservations).SingleAsync();
        var secondStock = await second.StockItems.Include(item => item.Reservations).SingleAsync();
        firstStock.Reserve(Guid.NewGuid(), 4);
        secondStock.Reserve(Guid.NewGuid(), 4);
        await first.SaveChangesAsync();
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());
    }

    [DockerFact]
    public async Task Payments_ShouldEnforceOnePaymentPerOrder()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:18-alpine").Build();
        await postgres.StartAsync();
        await using var context = new PaymentsPersistence.PaymentsDbContext(
            Options<PaymentsPersistence.PaymentsDbContext>(postgres.GetConnectionString())
        );
        await context.Database.MigrateAsync();
        var orderId = Guid.NewGuid();
        context.Payments.Add(
            OrderFlow.Payments.Domain.Payments.Payment.Create(
                orderId,
                10,
                OrderFlow.Payments.Domain.Payments.PaymentMethod.Card
            )
        );
        await context.SaveChangesAsync();
        context.Payments.Add(
            OrderFlow.Payments.Domain.Payments.Payment.Create(
                orderId,
                20,
                OrderFlow.Payments.Domain.Payments.PaymentMethod.Card
            )
        );
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [DockerFact]
    public async Task Notifications_ShouldQueryHistoryByOrder()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:18-alpine").Build();
        await postgres.StartAsync();
        await using var context = new NotificationsPersistence.NotificationsDbContext(
            Options<NotificationsPersistence.NotificationsDbContext>(postgres.GetConnectionString())
        );
        await context.Database.MigrateAsync();
        var orderId = Guid.NewGuid();
        context.Notifications.Add(
            OrderFlow.Notifications.Domain.Notifications.Notification.Create(
                orderId,
                Guid.NewGuid(),
                "OrderRecorded",
                "user@example.test",
                "Subject",
                "Message",
                OrderFlow.Notifications.Domain.Notifications.NotificationChannel.Email
            )
        );
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var records = await new NotificationsPersistence.NotificationRepository(
            context
        ).GetByOrderAsync(orderId, default);
        Assert.Single(records);
        Assert.Equal(orderId, records[0].OrderId);
    }
}
