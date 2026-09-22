using static OrderFlow.Inventory.Application.Inventory.InventoryFeatures;

namespace OrderFlow.Inventory.Api.Endpoints;

public sealed class GetInventoryByProductEndpoint(ISender sender)
    : Endpoint<GetInventoryByProductRequest, StockResponse>
{
    public override void Configure()
    {
        Get("/api/inventory/{productId}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(
        GetInventoryByProductRequest request,
        CancellationToken cancellationToken
    ) =>
        await Send.OkAsync(
            await sender.Send(new GetInventoryByProductQuery(request.ProductId), cancellationToken),
            cancellationToken
        );
}
