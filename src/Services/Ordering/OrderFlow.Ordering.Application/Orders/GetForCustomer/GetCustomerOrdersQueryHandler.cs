using MediatR;
using OrderFlow.Ordering.Application.Abstractions.Errors;
using OrderFlow.Ordering.Application.Abstractions.Messaging;
using OrderFlow.Ordering.Application.Abstractions.Persistence;
using OrderFlow.Ordering.Domain.Orders;

namespace OrderFlow.Ordering.Application.Orders;

public static partial class OrderFeatures
{
    public sealed class GetCustomerOrdersQueryHandler(IOrderRepository r)
        : IRequestHandler<GetCustomerOrdersQuery, IReadOnlyList<OrderResponse>>
    {
        public async Task<IReadOnlyList<OrderResponse>> Handle(
            GetCustomerOrdersQuery q,
            CancellationToken ct
        ) => (await r.GetCustomerOrdersAsync(q.CustomerId, ct)).Select(Map).ToArray();
    }
}
