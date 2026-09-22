using F = OrderFlow.Inventory.Application.Inventory.InventoryFeatures;

namespace OrderFlow.Inventory.Api.Endpoints;

public sealed class GetInventoryEndpoint(ISender sender)
    : Endpoint<GetInventoryRequest, IReadOnlyList<F.StockResponse>>
{
    public override void Configure()
    {
        Get("/api/inventory");
        AllowAnonymous();
    }

    public override async Task HandleAsync(
        GetInventoryRequest request,
        CancellationToken cancellationToken
    ) =>
        await Send.OkAsync(
            await sender.Send(
                new F.GetInventoryQuery(request.Page, request.PageSize),
                cancellationToken
            ),
            cancellationToken
        );
}
