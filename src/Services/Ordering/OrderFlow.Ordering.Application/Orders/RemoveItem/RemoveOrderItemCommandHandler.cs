using MediatR;
using OrderFlow.Ordering.Application.Abstractions.Errors;
using OrderFlow.Ordering.Application.Abstractions.Messaging;
using OrderFlow.Ordering.Application.Abstractions.Persistence;
using OrderFlow.Ordering.Domain.Orders;

namespace OrderFlow.Ordering.Application.Orders;

public static partial class OrderFeatures
{
    public sealed class RemoveOrderItemCommandHandler(IOrderRepository r)
        : IRequestHandler<RemoveOrderItemCommand, OrderResponse>
    {
        public async Task<OrderResponse> Handle(RemoveOrderItemCommand c, CancellationToken ct)
        {
            var x = await Find(r, c.OrderId, ct);
            x.RemoveItem(c.ItemId);
            await r.SaveAsync(ct);
            return Map(x);
        }
    }
}
