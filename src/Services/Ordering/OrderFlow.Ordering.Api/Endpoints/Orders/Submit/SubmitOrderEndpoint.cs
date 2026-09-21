using FastEndpoints;
using MediatR;
using F = OrderFlow.Ordering.Application.Orders.OrderFeatures;

namespace OrderFlow.Ordering.Api.Endpoints;

public static partial class OrderEndpoints
{
    public sealed class SubmitOrderEndpoint(ISender s) : Endpoint<OrderIdRequest, F.OrderResponse>
    {
        public override void Configure()
        {
            Post("/api/orders/{id}/submit");
            AllowAnonymous();
        }

        public override async Task HandleAsync(OrderIdRequest r, CancellationToken ct) =>
            await Send.OkAsync(await s.Send(new F.SubmitOrderCommand(r.Id), ct), ct);
    }
}
