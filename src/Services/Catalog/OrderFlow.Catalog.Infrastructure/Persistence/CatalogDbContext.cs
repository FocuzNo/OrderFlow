using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Categories;
using OrderFlow.Catalog.Domain.Common;
using OrderFlow.Catalog.Domain.Products;
using OrderFlow.IntegrationContracts;

namespace OrderFlow.Catalog.Infrastructure.Persistence;

public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options)
    : DbContext(options),
        IUnitOfWork
{
    public DbSet<Product> Products => Set<Product>();

    public DbSet<Category> Categories => Set<Category>();

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
                    ProductCreatedDomainEvent candidate =>
                        new CatalogProductCreatedIntegrationEvent(
                            domainEvent.EventId,
                            domainEvent.OccurredOnUtc,
                            candidate.ProductId,
                            candidate.Sku,
                            candidate.Name,
                            candidate.Price
                        ),
                    ProductPriceChangedDomainEvent candidate =>
                        new CatalogProductPriceChangedIntegrationEvent(
                            domainEvent.EventId,
                            domainEvent.OccurredOnUtc,
                            candidate.ProductId,
                            candidate.OldPrice,
                            candidate.NewPrice
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
                        Type = KafkaTopics.CatalogEvents,
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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
}
