using OrderFlow.Ordering.Application.Abstractions.Messaging;

namespace OrderFlow.Ordering.Application.Orders;

public static partial class OrderFeatures
{
    public sealed record GetOrdersQuery(int Page = 1, int PageSize = 20)
        : IQuery<IReadOnlyList<OrderResponse>>;
}
