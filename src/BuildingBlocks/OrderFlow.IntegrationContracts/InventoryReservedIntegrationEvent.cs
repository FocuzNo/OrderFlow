namespace OrderFlow.IntegrationContracts;

public sealed record InventoryReservedIntegrationEvent(Guid OrderId, IReadOnlyList<Guid> ReservationIds);
