using F = OrderFlow.Ordering.Application.Orders.OrderFeatures;

namespace OrderFlow.Ordering.Api.Endpoints;

public static partial class OrderEndpoints
{
    public sealed class GetOrderByIdEndpoint(ISender sender)
        : Endpoint<OrderIdRequest, F.OrderResponse>
    {
        public override void Configure()
        {
            Get("/api/orders/{id}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            OrderIdRequest request,
            CancellationToken cancellationToken
        ) =>
            await Send.OkAsync(
                await sender.Send(new F.GetOrderByIdQuery(request.Id), cancellationToken),
                cancellationToken
            );
    }
}
