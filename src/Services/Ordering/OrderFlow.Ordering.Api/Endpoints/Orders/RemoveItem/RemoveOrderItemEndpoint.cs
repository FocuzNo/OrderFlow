using F = OrderFlow.Ordering.Application.Orders.OrderFeatures;

namespace OrderFlow.Ordering.Api.Endpoints;

public static partial class OrderEndpoints
{
    public sealed class RemoveOrderItemEndpoint(ISender sender)
        : Endpoint<RemoveOrderItemRequest, F.OrderResponse>
    {
        public override void Configure()
        {
            Delete("/api/orders/{id}/items/{itemId}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            RemoveOrderItemRequest request,
            CancellationToken cancellationToken
        ) =>
            await Send.OkAsync(
                await sender.Send(
                    new F.RemoveOrderItemCommand(request.Id, request.ItemId),
                    cancellationToken
                ),
                cancellationToken
            );
    }
}
