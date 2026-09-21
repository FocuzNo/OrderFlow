using FastEndpoints;
using MediatR;
using F = OrderFlow.Inventory.Application.Inventory.InventoryFeatures;

namespace OrderFlow.Inventory.Api.Endpoints;

public static partial class InventoryEndpoints
{
    public sealed class GetStockItemEndpoint(ISender s)
        : Endpoint<GetStockItemRequest, F.StockResponse>
    {
        public override void Configure()
        {
            Get("/api/stock-items/{productId}/{warehouseId}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GetStockItemRequest r, CancellationToken ct) =>
            await Send.OkAsync(
                await s.Send(new F.GetStockItemQuery(r.ProductId, r.WarehouseId), ct),
                ct
            );
    }
}
