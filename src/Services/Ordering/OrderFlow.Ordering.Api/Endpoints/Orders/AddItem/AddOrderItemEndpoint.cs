using FastEndpoints;
using MediatR;
using F = OrderFlow.Ordering.Application.Orders.OrderFeatures;

namespace OrderFlow.Ordering.Api.Endpoints;

public static partial class OrderEndpoints
{
    public sealed class AddOrderItemEndpoint(ISender s)
        : Endpoint<AddOrderItemRequest, F.OrderResponse>
    {
        public override void Configure()
        {
            Post("/api/orders/{id}/items");
            AllowAnonymous();
        }

        public override async Task HandleAsync(AddOrderItemRequest r, CancellationToken ct) =>
            await Send.OkAsync(
                await s.Send(
                    new F.AddOrderItemCommand(
                        r.Id,
                        r.ProductId,
                        r.ProductName,
                        r.UnitPrice,
                        r.Quantity
                    ),
                    ct
                ),
                ct
            );
    }
}
