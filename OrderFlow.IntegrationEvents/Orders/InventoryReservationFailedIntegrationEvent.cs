namespace OrderFlow.IntegrationEvents.Inventory;

public sealed record InventoryReservationFailedIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid OrderId,
    string Reason
);
