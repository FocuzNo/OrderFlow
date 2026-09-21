using FastEndpoints;
using MediatR;
using F = OrderFlow.Inventory.Application.Inventory.InventoryFeatures;

namespace OrderFlow.Inventory.Api.Endpoints;

public static partial class InventoryEndpoints
{
    public sealed class GetWarehousesEndpoint(ISender s)
        : EndpointWithoutRequest<IReadOnlyList<F.WarehouseResponse>>
    {
        public override void Configure()
        {
            Get("/api/warehouses");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken ct) =>
            await Send.OkAsync(await s.Send(new F.GetWarehousesQuery(), ct), ct);
    }
}
