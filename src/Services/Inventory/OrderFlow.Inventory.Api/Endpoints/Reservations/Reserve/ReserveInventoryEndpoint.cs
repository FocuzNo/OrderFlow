using F = OrderFlow.Inventory.Application.Inventory.InventoryFeatures;

namespace OrderFlow.Inventory.Api.Endpoints;

public static partial class InventoryEndpoints
{
    public sealed class ReserveInventoryEndpoint(ISender sender)
        : Endpoint<ReserveInventoryRequest, F.ReservationResponse>
    {
        public override void Configure()
        {
            Post("/api/reservations");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            ReserveInventoryRequest request,
            CancellationToken cancellationToken
        ) =>
            await Send.ResponseAsync(
                await sender.Send(
                    new F.ReserveInventoryCommand(
                        request.ProductId,
                        request.WarehouseId,
                        request.OrderId,
                        request.Quantity
                    ),
                    cancellationToken
                ),
                201,
                cancellationToken
            );
    }
}
