namespace OrderFlow.IntegrationContracts;

public sealed record CatalogProductPriceChangedIntegrationEvent(Guid ProductId, decimal OldPrice, decimal NewPrice);
