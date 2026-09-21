using FastEndpoints;
using MediatR;
using F = OrderFlow.Ordering.Application.Orders.OrderFeatures;

namespace OrderFlow.Ordering.Api.Endpoints;

public static partial class OrderEndpoints
{
    public sealed class CreateOrderEndpoint(ISender s)
        : Endpoint<CreateOrderRequest, F.OrderResponse>
    {
        public override void Configure()
        {
            Post("/api/orders");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CreateOrderRequest r, CancellationToken ct) =>
            await Send.ResponseAsync(
                await s.Send(
                    new F.CreateOrderCommand(
                        r.CustomerId,
                        r.CustomerEmail,
                        new(r.Line1, r.City, r.PostalCode, r.Country)
                    ),
                    ct
                ),
                201,
                ct
            );
    }
}
