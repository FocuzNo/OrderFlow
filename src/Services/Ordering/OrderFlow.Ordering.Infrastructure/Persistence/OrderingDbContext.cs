using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrderFlow.IntegrationContracts;
using OrderFlow.Ordering.Application.Abstractions.Persistence;
using OrderFlow.Ordering.Domain.Common;
using OrderFlow.Ordering.Domain.Orders;

namespace OrderFlow.Ordering.Infrastructure.Persistence;

public sealed class OrderingDbContext(DbContextOptions<OrderingDbContext> options)
    : DbContext(options),
        IUnitOfWork
{
    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var roots = ChangeTracker
            .Entries<AggregateRoot>()
            .Select(candidate => candidate.Entity)
            .Where(candidate => candidate.DomainEvents.Count > 0)
            .ToArray();
        foreach (var root in roots)
        {
            foreach (var domainEvent in root.DomainEvents)
            {
                object? payload = domainEvent switch
                {
                    OrderSubmittedDomainEvent candidate => new OrderSubmittedIntegrationEvent(
                        domainEvent.EventId,
                        domainEvent.OccurredOnUtc,
                        candidate.OrderId,
                        candidate.CustomerId,
                        candidate.CustomerEmail,
                        candidate
                            .Items.Select(i => new OrderItemContract(
                                i.ProductId,
                                i.ProductName,
                                i.UnitPrice,
                                i.Quantity
                            ))
                            .ToArray(),
                        candidate.TotalAmount
                    ),
                    PaymentRequestedDomainEvent candidate => new PaymentRequestedIntegrationEvent(
                        domainEvent.EventId,
                        domainEvent.OccurredOnUtc,
                        candidate.OrderId,
                        candidate.Amount
                    ),
                    OrderConfirmedDomainEvent candidate => new OrderConfirmedIntegrationEvent(
                        domainEvent.EventId,
                        domainEvent.OccurredOnUtc,
                        candidate.OrderId,
                        candidate.CustomerId,
                        candidate.CustomerEmail
                    ),
                    OrderCancelledDomainEvent candidate => new OrderCancelledIntegrationEvent(
                        domainEvent.EventId,
                        domainEvent.OccurredOnUtc,
                        candidate.OrderId,
                        candidate.CustomerEmail,
                        candidate.Reason
                    ),
                    _ => null,
                };
                if (payload is null)
                    continue;
                var envelope = new IntegrationEventEnvelope(
                    domainEvent.EventId,
                    payload.GetType().Name,
                    1,
                    domainEvent.OccurredOnUtc,
                    System.Diagnostics.Activity.Current?.TraceId.ToString()
                        ?? domainEvent.EventId.ToString(),
                    System.Diagnostics.Activity.Current?.SpanId.ToString(),
                    root.Id.ToString(),
                    JsonSerializer.Serialize(payload, payload.GetType())
                );
                OutboxMessages.Add(
                    new()
                    {
                        Id = domainEvent.EventId,
                        Type = KafkaTopics.OrderEvents,
                        AggregateId = root.Id.ToString(),
                        OccurredOnUtc = domainEvent.OccurredOnUtc,
                        Content = JsonSerializer.Serialize(envelope),
                    }
                );
            }
            root.ClearDomainEvents();
        }
        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderingDbContext).Assembly);
}
