using OrderFlow.Inventory.Application.Abstractions.Messaging;

namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed record UpdateInventoryCommand(Guid ProductId, int Quantity)
        : ICommand<StockResponse>;
}
