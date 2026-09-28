namespace OrderFlow.Inventory.Application.Inventory;

public sealed record ReserveOrderInventoryItem(
    Guid ProductId,
    int Quantity
);
