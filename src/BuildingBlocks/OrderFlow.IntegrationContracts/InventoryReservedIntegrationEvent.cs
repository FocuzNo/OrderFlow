namespace OrderFlow.IntegrationContracts;

public sealed record InventoryReservedIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid OrderId,
    IReadOnlyList<Guid> ReservationIds
);
