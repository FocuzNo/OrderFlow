using OrderFlow.Ordering.Application.Abstractions.Errors;
using OrderFlow.Ordering.Application.Abstractions.Messaging;
using OrderFlow.Ordering.Application.Abstractions.Persistence;
using OrderFlow.Ordering.Domain.Orders;

namespace OrderFlow.Ordering.Application.Orders;

public static partial class OrderFeatures
{
    public sealed class GetOrderByIdQueryHandler(IOrderRepository repository)
        : IRequestHandler<GetOrderByIdQuery, OrderResponse>
    {
        public async Task<OrderResponse> Handle(
            GetOrderByIdQuery q,
            CancellationToken cancellationToken
        ) => Map(await Find(repository, q.Id, cancellationToken));
    }
}
