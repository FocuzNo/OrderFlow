using OrderFlow.Catalog.Domain.Common;
namespace OrderFlow.Catalog.Domain.Products;
public sealed record ProductCreatedDomainEvent(Guid EventId, DateTimeOffset OccurredOnUtc, Guid ProductId, string Sku, string Name, decimal Price) : IDomainEvent;
