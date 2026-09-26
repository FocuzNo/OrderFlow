using OrderFlow.Ordering.Application.Abstractions.Persistence;
using OrderFlow.Ordering.Domain.Orders;
using OrderFlow.Ordering.Infrastructure.Persistence.Outbox;

namespace OrderFlow.Ordering.Infrastructure.Persistence;

public sealed class OrderingDbContext(DbContextOptions<OrderingDbContext> options)
    : DbContext(options),
        IUnitOfWork
{
    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<OutboxMessage> OutboxMessages =>
        Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderingDbContext).Assembly);
}
