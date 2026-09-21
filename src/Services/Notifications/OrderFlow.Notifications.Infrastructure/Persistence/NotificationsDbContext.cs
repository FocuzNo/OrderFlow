using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrderFlow.IntegrationContracts;
using OrderFlow.Notifications.Domain.Common;
using OrderFlow.Notifications.Domain.Notifications;

namespace OrderFlow.Notifications.Infrastructure.Persistence;

public sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> o)
    : DbContext(o)
{
    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<NotificationDeliveryAttempt> Attempts => Set<NotificationDeliveryAttempt>();

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
                    NotificationSentDomainEvent x => new NotificationSentIntegrationEvent(
                        x.NotificationId,
                        x.Recipient
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
                        Type = KafkaTopics.NotificationEvents,
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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationsDbContext).Assembly);
}
