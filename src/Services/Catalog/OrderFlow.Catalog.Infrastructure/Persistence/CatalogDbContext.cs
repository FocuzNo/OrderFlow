using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Catalog.Domain.Categories;
using OrderFlow.Catalog.Domain.Common;
using OrderFlow.Catalog.Domain.Products;
using OrderFlow.IntegrationContracts;

namespace OrderFlow.Catalog.Infrastructure.Persistence;

public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> o) : DbContext(o)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
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
                    ProductCreatedDomainEvent x => new CatalogProductCreatedIntegrationEvent(
                        x.ProductId,
                        x.Sku,
                        x.Name,
                        x.Price
                    ),
                    ProductPriceChangedDomainEvent x =>
                        new CatalogProductPriceChangedIntegrationEvent(
                            x.ProductId,
                            x.OldPrice,
                            x.NewPrice
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
                        Type = KafkaTopics.CatalogEvents,
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

    protected override void OnModelCreating(ModelBuilder m)
    {
        m.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
        m.Entity<OutboxMessage>(b =>
        {
            b.ToTable("outbox_messages");
            b.HasKey(x => x.Id);
            b.Property(x => x.Type).HasMaxLength(250);
            b.Property(x => x.Content).HasColumnType("jsonb");
            b.Property(x => x.AggregateId).HasMaxLength(100);
            b.Property(x => x.Error).HasMaxLength(2000);
            b.HasIndex(x => new { x.ProcessedOnUtc, x.OccurredOnUtc });
        });
        m.Entity<InboxMessage>(b =>
        {
            b.ToTable("inbox_messages");
            b.HasKey(x => new { x.Id, x.Consumer });
            b.Property(x => x.Consumer).HasMaxLength(200);
        });
    }
}
