namespace OrderFlow.IntegrationContracts;

public sealed record CatalogProductPriceChangedIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid ProductId,
    decimal OldPrice,
    decimal NewPrice
);
