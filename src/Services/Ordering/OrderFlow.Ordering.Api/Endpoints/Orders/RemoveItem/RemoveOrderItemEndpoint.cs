using FastEndpoints;
using MediatR;
using F = OrderFlow.Ordering.Application.Orders.OrderFeatures;

namespace OrderFlow.Ordering.Api.Endpoints;

public static partial class OrderEndpoints
{
    public sealed class RemoveOrderItemEndpoint(ISender s)
        : Endpoint<RemoveOrderItemRequest, F.OrderResponse>
    {
        public override void Configure()
        {
            Delete("/api/orders/{id}/items/{itemId}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(RemoveOrderItemRequest r, CancellationToken ct) =>
            await Send.OkAsync(await s.Send(new F.RemoveOrderItemCommand(r.Id, r.ItemId), ct), ct);
    }
}
