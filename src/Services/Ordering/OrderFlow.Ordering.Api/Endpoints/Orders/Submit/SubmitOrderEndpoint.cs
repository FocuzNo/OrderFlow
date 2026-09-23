using F = OrderFlow.Ordering.Application.Orders.OrderFeatures;

namespace OrderFlow.Ordering.Api.Endpoints;

public static partial class OrderEndpoints
{
    public sealed class SubmitOrderEndpoint(ISender sender)
        : Endpoint<OrderIdRequest, F.OrderResponse>
    {
        public override void Configure()
        {
            Post("/api/orders/{id}/submit");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            OrderIdRequest request,
            CancellationToken cancellationToken
        ) =>
            await Send.OkAsync(
                await sender.Send(new F.SubmitOrderCommand(request.Id), cancellationToken),
                cancellationToken
            );
    }
}
