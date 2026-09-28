namespace OrderFlow.IntegrationEvents.Inventory;

public sealed record InventoryReservedItem(
    Guid ProductId,
    Guid StockItemId,
    Guid WarehouseId,
    Guid ReservationId,
    int Quantity
);
