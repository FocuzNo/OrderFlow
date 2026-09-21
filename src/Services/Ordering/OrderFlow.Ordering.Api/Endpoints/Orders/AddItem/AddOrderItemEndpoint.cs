using F = OrderFlow.Ordering.Application.Orders.OrderFeatures;

namespace OrderFlow.Ordering.Api.Endpoints;

public static partial class OrderEndpoints
{
    public sealed class AddOrderItemEndpoint(ISender sender)
        : Endpoint<AddOrderItemRequest, F.OrderResponse>
    {
        public override void Configure()
        {
            Post("/api/orders/{id}/items");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            AddOrderItemRequest request,
            CancellationToken cancellationToken
        ) =>
            await Send.OkAsync(
                await sender.Send(
                    new F.AddOrderItemCommand(
                        request.Id,
                        request.ProductId,
                        request.ProductName,
                        request.UnitPrice,
                        request.Quantity
                    ),
                    cancellationToken
                ),
                cancellationToken
            );
    }
}
