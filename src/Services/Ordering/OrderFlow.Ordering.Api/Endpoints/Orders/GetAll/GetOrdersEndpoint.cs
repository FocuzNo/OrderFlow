using F = OrderFlow.Ordering.Application.Orders.OrderFeatures;

namespace OrderFlow.Ordering.Api.Endpoints;

public sealed class GetOrdersEndpoint(ISender sender)
    : Endpoint<GetOrdersRequest, IReadOnlyList<F.OrderResponse>>
{
    public override void Configure()
    {
        Get("/api/orders");
        AllowAnonymous();
    }

    public override async Task HandleAsync(
        GetOrdersRequest request,
        CancellationToken cancellationToken
    ) =>
        await Send.OkAsync(
            await sender.Send(
                new F.GetOrdersQuery(request.Page, request.PageSize),
                cancellationToken
            ),
            cancellationToken
        );
}
