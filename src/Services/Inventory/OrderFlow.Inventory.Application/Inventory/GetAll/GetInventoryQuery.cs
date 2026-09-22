using OrderFlow.Inventory.Application.Abstractions.Messaging;

namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed record GetInventoryQuery(int Page = 1, int PageSize = 20)
        : IQuery<IReadOnlyList<StockResponse>>;
}
