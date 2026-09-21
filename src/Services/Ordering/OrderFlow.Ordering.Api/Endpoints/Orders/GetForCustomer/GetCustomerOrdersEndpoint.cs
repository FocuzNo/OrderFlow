using FastEndpoints;
using MediatR;
using F = OrderFlow.Ordering.Application.Orders.OrderFeatures;

namespace OrderFlow.Ordering.Api.Endpoints;

public static partial class OrderEndpoints
{
    public sealed class GetCustomerOrdersEndpoint(ISender s)
        : Endpoint<CustomerIdRequest, IReadOnlyList<F.OrderResponse>>
    {
        public override void Configure()
        {
            Get("/api/customers/{id}/orders");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CustomerIdRequest r, CancellationToken ct) =>
            await Send.OkAsync(await s.Send(new F.GetCustomerOrdersQuery(r.Id), ct), ct);
    }
}
