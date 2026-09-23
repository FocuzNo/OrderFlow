using F = OrderFlow.Ordering.Application.Orders.OrderFeatures;

namespace OrderFlow.Ordering.Api.Endpoints;

public static partial class OrderEndpoints
{
    public sealed class CreateOrderEndpoint(ISender sender)
        : Endpoint<CreateOrderRequest, F.OrderResponse>
    {
        public override void Configure()
        {
            Post("/api/orders");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            CreateOrderRequest request,
            CancellationToken cancellationToken
        ) =>
            await Send.ResponseAsync(
                await sender.Send(
                    new F.CreateOrderCommand(
                        request.CustomerId,
                        request.CustomerEmail,
                        new(request.Line1, request.City, request.PostalCode, request.Country),
                        (request.Items ?? [])
                            .Select(item =>
                                item is null
                                    ? null!
                                    : new F.OrderItemInput(
                                        item.ProductId,
                                        item.ProductName,
                                        item.UnitPrice,
                                        item.Quantity
                                    )
                            )
                            .ToArray()
                    ),
                    cancellationToken
                ),
                201,
                cancellationToken
            );
    }
}
