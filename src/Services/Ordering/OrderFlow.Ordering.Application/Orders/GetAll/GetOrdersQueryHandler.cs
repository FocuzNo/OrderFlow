using OrderFlow.Ordering.Application.Abstractions.Persistence;

namespace OrderFlow.Ordering.Application.Orders;

public static partial class OrderFeatures
{
    public sealed class GetOrdersQueryHandler(IOrderRepository repository)
        : IRequestHandler<GetOrdersQuery, IReadOnlyList<OrderResponse>>
    {
        public async Task<IReadOnlyList<OrderResponse>> Handle(
            GetOrdersQuery query,
            CancellationToken cancellationToken
        ) =>
            (await repository.ListAsync(query.Page, query.PageSize, cancellationToken))
                .Select(Map)
                .ToArray();
    }
}
