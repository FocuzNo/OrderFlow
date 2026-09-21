using FastEndpoints;
using MediatR;
using F = OrderFlow.Inventory.Application.Inventory.InventoryFeatures;

namespace OrderFlow.Inventory.Api.Endpoints;

public static partial class InventoryEndpoints
{
    public sealed class ReleaseReservationEndpoint(ISender s)
        : Endpoint<ChangeReservationStateRequest>
    {
        public override void Configure()
        {
            Post("/api/reservations/{reservationId}/release");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            ChangeReservationStateRequest r,
            CancellationToken ct
        )
        {
            await s.Send(
                new F.ReleaseReservationCommand(r.ProductId, r.WarehouseId, r.ReservationId),
                ct
            );
            await Send.NoContentAsync(ct);
        }
    }
}
