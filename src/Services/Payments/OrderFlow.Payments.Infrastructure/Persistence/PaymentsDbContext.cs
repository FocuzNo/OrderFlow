using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrderFlow.IntegrationContracts;
using OrderFlow.Payments.Domain.Common;
using OrderFlow.Payments.Domain.Payments;

namespace OrderFlow.Payments.Infrastructure.Persistence;

public sealed class PaymentsDbContext(DbContextOptions<PaymentsDbContext> o) : DbContext(o)
{
    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<Refund> Refunds => Set<Refund>();

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
                object? p = e switch
                {
                    PaymentRequestedDomainEvent x => new PaymentRequestedIntegrationEvent(
                        x.OrderId,
                        x.Amount
                    ),
                    PaymentSucceededDomainEvent x => new PaymentSucceededIntegrationEvent(
                        x.PaymentId,
                        x.OrderId,
                        x.Amount
                    ),
                    PaymentFailedDomainEvent x => new PaymentFailedIntegrationEvent(
                        x.PaymentId,
                        x.OrderId,
                        x.Reason
                    ),
                    _ => null,
                };
                if (p is null)
                    continue;
                var env = new IntegrationEventEnvelope(
                    e.EventId,
                    p.GetType().Name,
                    1,
                    e.OccurredOnUtc,
                    System.Diagnostics.Activity.Current?.TraceId.ToString() ?? e.EventId.ToString(),
                    System.Diagnostics.Activity.Current?.SpanId.ToString(),
                    root.Id.ToString(),
                    JsonSerializer.Serialize(p, p.GetType())
                );
                OutboxMessages.Add(
                    new()
                    {
                        Id = e.EventId,
                        Type = KafkaTopics.PaymentEvents,
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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentsDbContext).Assembly);
}
