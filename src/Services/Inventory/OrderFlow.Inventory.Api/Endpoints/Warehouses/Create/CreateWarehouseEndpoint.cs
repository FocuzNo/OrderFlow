using FastEndpoints;
using MediatR;
using F = OrderFlow.Inventory.Application.Inventory.InventoryFeatures;

namespace OrderFlow.Inventory.Api.Endpoints;

public static partial class InventoryEndpoints
{
    public sealed class CreateWarehouseEndpoint(ISender s)
        : Endpoint<CreateWarehouseRequest, F.WarehouseResponse>
    {
        public override void Configure()
        {
            Post("/api/warehouses");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CreateWarehouseRequest r, CancellationToken ct) =>
            await Send.ResponseAsync(
                await s.Send(new F.CreateWarehouseCommand(r.Name, r.Location), ct),
                201,
                ct
            );
    }
}
