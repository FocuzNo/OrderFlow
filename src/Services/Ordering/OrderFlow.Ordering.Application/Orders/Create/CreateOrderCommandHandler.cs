using MediatR;
using OrderFlow.Ordering.Application.Abstractions.Errors;
using OrderFlow.Ordering.Application.Abstractions.Messaging;
using OrderFlow.Ordering.Application.Abstractions.Persistence;
using OrderFlow.Ordering.Domain.Orders;

namespace OrderFlow.Ordering.Application.Orders;

public static partial class OrderFeatures
{
    public sealed class CreateOrderCommandHandler(IOrderRepository r)
        : IRequestHandler<CreateOrderCommand, OrderResponse>
    {
        public async Task<OrderResponse> Handle(CreateOrderCommand c, CancellationToken ct)
        {
            var a = ShippingAddress.Create(
                c.ShippingAddress.Line1,
                c.ShippingAddress.City,
                c.ShippingAddress.PostalCode,
                c.ShippingAddress.Country
            );
            var x = Order.Create(c.CustomerId, c.CustomerEmail, a);
            await r.AddAsync(x, ct);
            return Map(x);
        }
    }
}
