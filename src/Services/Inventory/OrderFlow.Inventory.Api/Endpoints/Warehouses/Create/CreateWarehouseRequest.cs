using F = OrderFlow.Inventory.Application.Inventory.InventoryFeatures;

namespace OrderFlow.Inventory.Api.Endpoints;

public static partial class InventoryEndpoints
{
    public sealed record CreateWarehouseRequest
    {
        public string Name { get; init; } = string.Empty;

        public string Location { get; init; } = string.Empty;
    }
}
