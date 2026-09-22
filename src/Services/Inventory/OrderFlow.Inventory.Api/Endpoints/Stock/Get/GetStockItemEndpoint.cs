using F = OrderFlow.Inventory.Application.Inventory.InventoryFeatures;

namespace OrderFlow.Inventory.Api.Endpoints;

public static partial class InventoryEndpoints
{
    public sealed class GetStockItemEndpoint(ISender sender)
        : Endpoint<GetStockItemRequest, F.StockResponse>
    {
        public override void Configure()
        {
            Get("/api/stock-items/{productId}/{warehouseId}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            GetStockItemRequest request,
            CancellationToken cancellationToken
        ) =>
            await Send.OkAsync(
                await sender.Send(
                    new F.GetStockItemQuery(request.ProductId, request.WarehouseId),
                    cancellationToken
                ),
                cancellationToken
            );
    }
}
