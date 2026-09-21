using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrderFlow.IntegrationContracts;
using OrderFlow.Ordering.Domain.Common;
using OrderFlow.Ordering.Domain.Orders;

namespace OrderFlow.Ordering.Infrastructure.Persistence;

public sealed class OrderingDbContext(DbContextOptions<OrderingDbContext> options)
    : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var roots = ChangeTracker
            .Entries<AggregateRoot>()
            .Select(x => x.Entity)
            .Where(x => x.DomainEvents.Count > 0)
            .ToArray();
        foreach (var root in roots)
        {
            foreach (var e in root.DomainEvents)
            {
                object? payload = e switch
                {
                    OrderSubmittedDomainEvent x => new OrderSubmittedIntegrationEvent(
                        x.OrderId,
                        x.CustomerId,
                        x.CustomerEmail,
                        x.Items.Select(i => new OrderItemContract(
                                i.ProductId,
                                i.ProductName,
                                i.UnitPrice,
                                i.Quantity
                            ))
                            .ToArray(),
                        x.TotalAmount
                    ),
                    PaymentRequestedDomainEvent x => new PaymentRequestedIntegrationEvent(
                        x.OrderId,
                        x.Amount
                    ),
                    OrderConfirmedDomainEvent x => new OrderConfirmedIntegrationEvent(
                        x.OrderId,
                        x.CustomerId,
                        x.CustomerEmail
                    ),
                    OrderCancelledDomainEvent x => new OrderCancelledIntegrationEvent(
                        x.OrderId,
                        x.CustomerEmail,
                        x.Reason
                    ),
                    _ => null,
                };
                if (payload is null)
                    continue;
                var env = new IntegrationEventEnvelope(
                    e.EventId,
                    payload.GetType().Name,
                    1,
                    e.OccurredOnUtc,
                    System.Diagnostics.Activity.Current?.TraceId.ToString() ?? e.EventId.ToString(),
                    System.Diagnostics.Activity.Current?.SpanId.ToString(),
                    root.Id.ToString(),
                    JsonSerializer.Serialize(payload, payload.GetType())
                );
                OutboxMessages.Add(
                    new()
                    {
                        Id = e.EventId,
                        Type = KafkaTopics.OrderEvents,
                        AggregateId = root.Id.ToString(),
                        OccurredOnUtc = e.OccurredOnUtc,
                        Content = JsonSerializer.Serialize(env),
                    }
                );
            }
            root.ClearDomainEvents();
        }
        return await base.SaveChangesAsync(ct);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderingDbContext).Assembly);
}
