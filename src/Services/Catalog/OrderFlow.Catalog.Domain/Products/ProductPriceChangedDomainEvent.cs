using OrderFlow.Catalog.Domain.Common;

namespace OrderFlow.Catalog.Domain.Products;

public sealed record ProductPriceChangedDomainEvent(
    Guid EventId,
    DateTimeOffset OccurredOnUtc,
    Guid ProductId,
    decimal OldPrice,
    decimal NewPrice
) : IDomainEvent;
