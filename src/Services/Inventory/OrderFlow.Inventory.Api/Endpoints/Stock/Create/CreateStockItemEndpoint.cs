using F = OrderFlow.Inventory.Application.Inventory.InventoryFeatures;

namespace OrderFlow.Inventory.Api.Endpoints;

public static partial class InventoryEndpoints
{
    public sealed class CreateStockItemEndpoint(ISender sender)
        : Endpoint<CreateStockItemRequest, F.StockResponse>
    {
        public override void Configure()
        {
            Post("/api/stock-items");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            CreateStockItemRequest request,
            CancellationToken cancellationToken
        ) =>
            await Send.ResponseAsync(
                await sender.Send(
                    new F.CreateStockItemCommand(
                        request.ProductId,
                        request.WarehouseId,
                        request.Sku
                    ),
                    cancellationToken
                ),
                201,
                cancellationToken
            );
    }
}
