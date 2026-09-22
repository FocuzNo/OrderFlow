using F = OrderFlow.Inventory.Application.Inventory.InventoryFeatures;

namespace OrderFlow.Inventory.Api.Endpoints;

public static partial class InventoryEndpoints
{
    public sealed record ReserveInventoryRequest
    {
        public Guid ProductId { get; init; }

        public Guid WarehouseId { get; init; }

        public Guid OrderId { get; init; }

        public int Quantity { get; init; }
    }
}
