using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrderFlow.IntegrationContracts;
using OrderFlow.Notifications.Application.Abstractions.Persistence;
using OrderFlow.Notifications.Domain.Common;
using OrderFlow.Notifications.Domain.Notifications;

namespace OrderFlow.Notifications.Infrastructure.Persistence;

public sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options)
    : DbContext(options),
        IUnitOfWork
{
    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<NotificationDeliveryAttempt> Attempts => Set<NotificationDeliveryAttempt>();

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
                    NotificationSentDomainEvent candidate => new NotificationSentIntegrationEvent(
                        domainEvent.EventId,
                        domainEvent.OccurredOnUtc,
                        candidate.NotificationId,
                        candidate.Recipient
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
                        Type = KafkaTopics.NotificationEvents,
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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationsDbContext).Assembly);
}
