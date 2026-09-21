using FastEndpoints;
using MediatR;
using F = OrderFlow.Inventory.Application.Inventory.InventoryFeatures;

namespace OrderFlow.Inventory.Api.Endpoints;

public static partial class InventoryEndpoints
{
    public sealed class GetReservationByIdEndpoint(ISender s)
        : Endpoint<ReservationIdRequest, F.ReservationResponse>
    {
        public override void Configure()
        {
            Get("/api/reservations/{id}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(ReservationIdRequest r, CancellationToken ct) =>
            await Send.OkAsync(await s.Send(new F.GetReservationByIdQuery(r.Id), ct), ct);
    }
}
