using OrderFlow.Inventory.Application.Abstractions.Messaging;

namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed record DeleteInventoryCommand(Guid ProductId) : ICommand;
}
