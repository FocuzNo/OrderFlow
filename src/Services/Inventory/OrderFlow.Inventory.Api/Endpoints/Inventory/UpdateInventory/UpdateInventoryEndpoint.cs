using F = OrderFlow.Inventory.Application.Inventory.InventoryFeatures;

namespace OrderFlow.Inventory.Api.Endpoints;

public sealed class UpdateInventoryEndpoint(ISender sender)
    : Endpoint<UpdateInventoryRequest, F.StockResponse>
{
    public override void Configure()
    {
        Put("/api/inventory/{productId}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(
        UpdateInventoryRequest request,
        CancellationToken cancellationToken
    )
    {
        await Send.ResponseAsync(
            await sender.Send(
                new F.UpdateInventoryCommand(request.ProductId, request.Quantity),
                cancellationToken
            ),
            200,
            cancellationToken
        );
    }
}
