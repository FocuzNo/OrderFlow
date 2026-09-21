using MediatR;
using OrderFlow.Ordering.Application.Abstractions.Errors;
using OrderFlow.Ordering.Application.Abstractions.Messaging;
using OrderFlow.Ordering.Application.Abstractions.Persistence;
using OrderFlow.Ordering.Domain.Orders;

namespace OrderFlow.Ordering.Application.Orders;

public static partial class OrderFeatures
{
    public sealed record OrderResponse(
        Guid Id,
        Guid CustomerId,
        string CustomerEmail,
        ShippingAddressInput ShippingAddress,
        IReadOnlyList<OrderItemResponse> Items,
        decimal TotalAmount,
        string Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt
    );
}
