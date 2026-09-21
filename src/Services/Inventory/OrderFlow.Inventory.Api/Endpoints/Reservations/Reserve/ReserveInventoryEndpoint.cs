using FastEndpoints;
using MediatR;
using F = OrderFlow.Inventory.Application.Inventory.InventoryFeatures;

namespace OrderFlow.Inventory.Api.Endpoints;

public static partial class InventoryEndpoints
{
    public sealed class ReserveInventoryEndpoint(ISender s)
        : Endpoint<ReserveInventoryRequest, F.ReservationResponse>
    {
        public override void Configure()
        {
            Post("/api/reservations");
            AllowAnonymous();
        }

        public override async Task HandleAsync(ReserveInventoryRequest r, CancellationToken ct) =>
            await Send.ResponseAsync(
                await s.Send(
                    new F.ReserveInventoryCommand(
                        r.ProductId,
                        r.WarehouseId,
                        r.OrderId,
                        r.Quantity
                    ),
                    ct
                ),
                201,
                ct
            );
    }
}
