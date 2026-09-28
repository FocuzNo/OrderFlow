namespace OrderFlow.Inventory.Application.Inventory;

public sealed record ReservedStockItem(
    Guid ProductId,
    Guid StockItemId,
    Guid WarehouseId,
    Guid ReservationId,
    int Quantity
);
