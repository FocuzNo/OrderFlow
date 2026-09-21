using MediatR;
using OrderFlow.Ordering.Application.Abstractions.Errors;
using OrderFlow.Ordering.Application.Abstractions.Messaging;
using OrderFlow.Ordering.Application.Abstractions.Persistence;
using OrderFlow.Ordering.Domain.Orders;

namespace OrderFlow.Ordering.Application.Orders;

public static partial class OrderFeatures
{
    public sealed class AddOrderItemCommandHandler(IOrderRepository r)
        : IRequestHandler<AddOrderItemCommand, OrderResponse>
    {
        public async Task<OrderResponse> Handle(AddOrderItemCommand c, CancellationToken ct)
        {
            var x = await Find(r, c.OrderId, ct);
            x.AddItem(c.ProductId, c.ProductName, c.UnitPrice, c.Quantity);
            await r.SaveAsync(ct);
            return Map(x);
        }
    }
}
