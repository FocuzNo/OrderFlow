using FastEndpoints;
using MediatR;
using F = OrderFlow.Inventory.Application.Inventory.InventoryFeatures;

namespace OrderFlow.Inventory.Api.Endpoints;

public static partial class InventoryEndpoints
{
    public sealed class CreateStockItemRequest
    {
        public Guid ProductId { get; set; }

        public Guid WarehouseId { get; set; }

        public string Sku { get; set; } = string.Empty;
    }
}
