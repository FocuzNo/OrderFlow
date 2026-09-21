namespace OrderFlow.IntegrationContracts;

public sealed record CatalogProductCreatedIntegrationEvent(Guid ProductId, string Sku, string Name, decimal Price);
