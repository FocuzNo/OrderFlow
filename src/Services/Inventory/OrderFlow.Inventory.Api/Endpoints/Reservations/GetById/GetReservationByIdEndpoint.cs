using F = OrderFlow.Inventory.Application.Inventory.InventoryFeatures;

namespace OrderFlow.Inventory.Api.Endpoints;

public static partial class InventoryEndpoints
{
    public sealed class GetReservationByIdEndpoint(ISender sender)
        : Endpoint<ReservationIdRequest, F.ReservationResponse>
    {
        public override void Configure()
        {
            Get("/api/reservations/{id}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            ReservationIdRequest request,
            CancellationToken cancellationToken
        ) =>
            await Send.OkAsync(
                await sender.Send(new F.GetReservationByIdQuery(request.Id), cancellationToken),
                cancellationToken
            );
    }
}
