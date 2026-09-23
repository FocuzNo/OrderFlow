using F = OrderFlow.Inventory.Application.Inventory.InventoryFeatures;

namespace OrderFlow.Inventory.Api.Endpoints;

public sealed class CreateInventoryEndpoint(ISender sender)
    : Endpoint<CreateInventoryRequest, F.StockResponse>
{
    public override void Configure()
    {
        Post("/api/inventory");
        AllowAnonymous();
    }

    public override async Task HandleAsync(
        CreateInventoryRequest request,
        CancellationToken cancellationToken
    )
    {
        await Send.ResponseAsync(
            await sender.Send(
                new F.CreateInventoryCommand(
                    request.ProductId,
                    request.WarehouseId,
                    request.Sku,
                    request.Quantity
                ),
                cancellationToken
            ),
            201,
            cancellationToken
        );
    }
}
