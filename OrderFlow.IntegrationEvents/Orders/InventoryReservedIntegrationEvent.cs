namespace OrderFlow.IntegrationEvents.Inventory;

public sealed record InventoryReservedIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid OrderId,
    IReadOnlyCollection<InventoryReservedItem> Items
);
