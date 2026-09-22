using OrderFlow.Inventory.Application.Abstractions.Messaging;

namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed record CreateInventoryCommand(
        Guid ProductId,
        Guid WarehouseId,
        string Sku,
        int Quantity
    ) : ICommand<StockResponse>;
}
