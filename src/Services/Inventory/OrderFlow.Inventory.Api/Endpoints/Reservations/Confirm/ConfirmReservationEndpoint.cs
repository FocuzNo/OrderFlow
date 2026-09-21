using F = OrderFlow.Inventory.Application.Inventory.InventoryFeatures;

namespace OrderFlow.Inventory.Api.Endpoints;

public static partial class InventoryEndpoints
{
    public sealed class ConfirmReservationEndpoint(ISender sender)
        : Endpoint<ChangeReservationStateRequest>
    {
        public override void Configure()
        {
            Post("/api/reservations/{reservationId}/confirm");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            ChangeReservationStateRequest request,
            CancellationToken cancellationToken
        )
        {
            await sender.Send(
                new F.ConfirmReservationCommand(
                    request.ProductId,
                    request.WarehouseId,
                    request.ReservationId
                ),
                cancellationToken
            );
            await Send.NoContentAsync(cancellationToken);
        }
    }
}
