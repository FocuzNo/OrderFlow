using OrderFlow.Ordering.Application.Abstractions.Errors;
using OrderFlow.Ordering.Application.Abstractions.Messaging;
using OrderFlow.Ordering.Application.Abstractions.Persistence;
using OrderFlow.Ordering.Domain.Orders;

namespace OrderFlow.Ordering.Application.Orders;

public static partial class OrderFeatures
{
    public sealed class GetCustomerOrdersQueryHandler(IOrderRepository repository)
        : IRequestHandler<GetCustomerOrdersQuery, IReadOnlyList<OrderResponse>>
    {
        public async Task<IReadOnlyList<OrderResponse>> Handle(
            GetCustomerOrdersQuery q,
            CancellationToken cancellationToken
        ) =>
            (await repository.GetCustomerOrdersAsync(q.CustomerId, cancellationToken))
                .Select(Map)
                .ToArray();
    }
}
