namespace OrderFlow.IntegrationContracts;

public sealed record CatalogProductCreatedIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid ProductId,
    string Sku,
    string Name,
    decimal Price
);
