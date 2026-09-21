using FastEndpoints;
using MediatR;
using F = OrderFlow.Ordering.Application.Orders.OrderFeatures;

namespace OrderFlow.Ordering.Api.Endpoints;

public static partial class OrderEndpoints
{
    public sealed class GetOrderByIdEndpoint(ISender s) : Endpoint<OrderIdRequest, F.OrderResponse>
    {
        public override void Configure()
        {
            Get("/api/orders/{id}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(OrderIdRequest r, CancellationToken ct) =>
            await Send.OkAsync(await s.Send(new F.GetOrderByIdQuery(r.Id), ct), ct);
    }
}
