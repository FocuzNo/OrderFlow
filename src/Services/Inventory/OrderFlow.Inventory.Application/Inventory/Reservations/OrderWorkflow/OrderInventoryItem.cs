namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed record OrderInventoryItem(Guid ProductId, int Quantity);
}
