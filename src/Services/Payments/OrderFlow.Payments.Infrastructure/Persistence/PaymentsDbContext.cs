using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrderFlow.IntegrationContracts;
using OrderFlow.Payments.Application.Abstractions.Persistence;
using OrderFlow.Payments.Domain.Common;
using OrderFlow.Payments.Domain.Payments;

namespace OrderFlow.Payments.Infrastructure.Persistence;

public sealed class PaymentsDbContext(DbContextOptions<PaymentsDbContext> options)
    : DbContext(options),
        IUnitOfWork
{
    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<Refund> Refunds => Set<Refund>();

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
                    PaymentRequestedDomainEvent candidate => new PaymentRequestedIntegrationEvent(
                        domainEvent.EventId,
                        domainEvent.OccurredOnUtc,
                        candidate.OrderId,
                        candidate.Amount
                    ),
                    PaymentSucceededDomainEvent candidate => new PaymentSucceededIntegrationEvent(
                        domainEvent.EventId,
                        domainEvent.OccurredOnUtc,
                        candidate.PaymentId,
                        candidate.OrderId,
                        candidate.Amount
                    ),
                    PaymentFailedDomainEvent candidate => new PaymentFailedIntegrationEvent(
                        domainEvent.EventId,
                        domainEvent.OccurredOnUtc,
                        candidate.PaymentId,
                        candidate.OrderId,
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
                        Type = KafkaTopics.PaymentEvents,
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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentsDbContext).Assembly);
}
