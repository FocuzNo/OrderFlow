using OrderFlow.Catalog.Domain.Common;

namespace OrderFlow.Catalog.Domain.Products;

public sealed record ProductArchivedDomainEvent(
    Guid EventId,
    DateTimeOffset OccurredOnUtc,
    Guid ProductId
) : IDomainEvent;
