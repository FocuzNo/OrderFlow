using F = OrderFlow.Inventory.Application.Inventory.InventoryFeatures;

namespace OrderFlow.Inventory.Api.Endpoints;

public static partial class InventoryEndpoints
{
    public sealed class GetWarehousesEndpoint(ISender sender)
        : EndpointWithoutRequest<IReadOnlyList<F.WarehouseResponse>>
    {
        public override void Configure()
        {
            Get("/api/warehouses");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken cancellationToken) =>
            await Send.OkAsync(
                await sender.Send(new F.GetWarehousesQuery(), cancellationToken),
                cancellationToken
            );
    }
}
