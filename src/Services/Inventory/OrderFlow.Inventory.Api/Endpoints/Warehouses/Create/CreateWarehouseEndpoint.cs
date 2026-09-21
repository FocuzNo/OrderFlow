using F = OrderFlow.Inventory.Application.Inventory.InventoryFeatures;

namespace OrderFlow.Inventory.Api.Endpoints;

public static partial class InventoryEndpoints
{
    public sealed class CreateWarehouseEndpoint(ISender sender)
        : Endpoint<CreateWarehouseRequest, F.WarehouseResponse>
    {
        public override void Configure()
        {
            Post("/api/warehouses");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            CreateWarehouseRequest request,
            CancellationToken cancellationToken
        ) =>
            await Send.ResponseAsync(
                await sender.Send(
                    new F.CreateWarehouseCommand(request.Name, request.Location),
                    cancellationToken
                ),
                201,
                cancellationToken
            );
    }
}
