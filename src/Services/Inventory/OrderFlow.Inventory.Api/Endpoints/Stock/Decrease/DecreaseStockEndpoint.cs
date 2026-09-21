using F = OrderFlow.Inventory.Application.Inventory.InventoryFeatures;

namespace OrderFlow.Inventory.Api.Endpoints;

public static partial class InventoryEndpoints
{
    public sealed class DecreaseStockEndpoint(ISender sender)
        : Endpoint<AdjustStockRequest, F.StockResponse>
    {
        public override void Configure()
        {
            Post("/api/stock-items/decrease");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            AdjustStockRequest request,
            CancellationToken cancellationToken
        ) =>
            await Send.OkAsync(
                await sender.Send(
                    new F.DecreaseStockCommand(
                        request.ProductId,
                        request.WarehouseId,
                        request.Quantity
                    ),
                    cancellationToken
                ),
                cancellationToken
            );
    }
}
