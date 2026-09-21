using FastEndpoints;
using MediatR;
using F = OrderFlow.Ordering.Application.Orders.OrderFeatures;

namespace OrderFlow.Ordering.Api.Endpoints;

public static partial class OrderEndpoints
{
    public sealed class CancelOrderEndpoint(ISender s)
        : Endpoint<CancelOrderRequest, F.OrderResponse>
    {
        public override void Configure()
        {
            Post("/api/orders/{id}/cancel");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancelOrderRequest r, CancellationToken ct) =>
            await Send.OkAsync(await s.Send(new F.CancelOrderCommand(r.Id, r.Reason), ct), ct);
    }
}
