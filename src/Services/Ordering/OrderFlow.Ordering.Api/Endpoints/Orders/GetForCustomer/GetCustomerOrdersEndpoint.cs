using F = OrderFlow.Ordering.Application.Orders.OrderFeatures;

namespace OrderFlow.Ordering.Api.Endpoints;

public static partial class OrderEndpoints
{
    public sealed class GetCustomerOrdersEndpoint(ISender sender)
        : Endpoint<CustomerIdRequest, IReadOnlyList<F.OrderResponse>>
    {
        public override void Configure()
        {
            Get("/api/customers/{id}/orders");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            CustomerIdRequest request,
            CancellationToken cancellationToken
        ) =>
            await Send.OkAsync(
                await sender.Send(new F.GetCustomerOrdersQuery(request.Id), cancellationToken),
                cancellationToken
            );
    }
}
