using OrderFlow.Ordering.Application.Abstractions.Messaging;

namespace OrderFlow.Ordering.Application.Orders;

public static partial class OrderFeatures
{
    public sealed record CreateOrderCommand(
        Guid CustomerId,
        string CustomerEmail,
        ShippingAddressInput ShippingAddress,
        IReadOnlyList<OrderItemInput> Items) : ICommand<OrderResponse>;
}
