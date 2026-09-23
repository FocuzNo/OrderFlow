using F = OrderFlow.Ordering.Application.Orders.OrderFeatures;

namespace OrderFlow.Ordering.Api.Endpoints;

public static partial class OrderEndpoints
{
    public sealed class CancelOrderEndpoint(ISender sender)
        : Endpoint<CancelOrderRequest, F.OrderResponse>
    {
        public override void Configure()
        {
            Post("/api/orders/{id}/cancel");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            CancelOrderRequest request,
            CancellationToken cancellationToken
        ) =>
            await Send.OkAsync(
                await sender.Send(
                    new F.CancelOrderCommand(request.Id, request.Reason),
                    cancellationToken
                ),
                cancellationToken
            );
    }
}
