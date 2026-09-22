using F = OrderFlow.Inventory.Application.Inventory.InventoryFeatures;

namespace OrderFlow.Inventory.Api.Endpoints;

public sealed class DeleteInventoryEndpoint(ISender sender) : Endpoint<DeleteInventoryRequest>
{
    public override void Configure()
    {
        Delete("/api/inventory/{productId}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(
        DeleteInventoryRequest request,
        CancellationToken cancellationToken
    )
    {
        await sender.Send(new F.DeleteInventoryCommand(request.ProductId), cancellationToken);
        await Send.NoContentAsync(cancellationToken);
    }
}
