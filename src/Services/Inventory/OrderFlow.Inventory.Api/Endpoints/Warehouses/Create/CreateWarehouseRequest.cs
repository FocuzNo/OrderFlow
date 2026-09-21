using FastEndpoints;
using MediatR;
using F = OrderFlow.Inventory.Application.Inventory.InventoryFeatures;

namespace OrderFlow.Inventory.Api.Endpoints;

public static partial class InventoryEndpoints
{
    public sealed class CreateWarehouseRequest
    {
        public string Name { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;
    }
}
