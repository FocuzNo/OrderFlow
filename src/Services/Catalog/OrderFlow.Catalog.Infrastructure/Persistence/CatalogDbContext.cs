using Microsoft.EntityFrameworkCore;
using OrderFlow.Catalog.Domain;
using OrderFlow.Catalog.Domain.Products;
using System.Text.Json;

namespace OrderFlow.Catalog.Infrastructure.Persistence;

public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var aggregates = ChangeTracker.Entries<IHasDomainEvents>()
            .Select(entry => entry.Entity)
            .Where(entity => entity.DomainEvents.Count > 0)
            .ToArray();

        foreach (var aggregate in aggregates)
        {
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                OutboxMessages.Add(new OutboxMessage
                {
                    Id = domainEvent.Id,
                    Type = domainEvent.Type,
                    OccurredAt = domainEvent.OccurredAt,
                    Payload = JsonSerializer.Serialize(domainEvent)
                });
            }

            aggregate.ClearDomainEvents();
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.ToTable("outbox_messages");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Type).HasMaxLength(250).IsRequired();
            builder.Property(x => x.Payload).HasColumnType("jsonb").IsRequired();
            builder.Property(x => x.Error).HasMaxLength(2000);
            builder.HasIndex(x => new { x.ProcessedAt, x.OccurredAt });
        });
    }
}
