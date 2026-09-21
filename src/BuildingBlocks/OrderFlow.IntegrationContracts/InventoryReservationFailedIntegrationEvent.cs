namespace OrderFlow.IntegrationContracts;

public sealed record InventoryReservationFailedIntegrationEvent(Guid OrderId, string Reason);
