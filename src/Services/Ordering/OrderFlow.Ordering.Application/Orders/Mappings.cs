using MediatR;
using OrderFlow.Ordering.Application.Abstractions.Errors;
using OrderFlow.Ordering.Application.Abstractions.Messaging;
using OrderFlow.Ordering.Application.Abstractions.Persistence;
using OrderFlow.Ordering.Domain.Orders;

namespace OrderFlow.Ordering.Application.Orders;

public static partial class OrderFeatures
{
    private static async Task<Order> Find(IOrderRepository r, Guid id, CancellationToken ct) =>
        await r.GetAsync(id, ct) ?? throw new NotFoundException("Order was not found.");

    private static OrderResponse Map(Order x) =>
        new(
            x.Id,
            x.CustomerId,
            x.CustomerEmail.Value,
            new(
                x.ShippingAddress.Line1,
                x.ShippingAddress.City,
                x.ShippingAddress.PostalCode,
                x.ShippingAddress.Country
            ),
            x.Items.Select(i => new OrderItemResponse(
                    i.Id,
                    i.ProductId,
                    i.ProductName,
                    i.UnitPrice,
                    i.Quantity,
                    i.Total
                ))
                .ToArray(),
            x.TotalAmount,
            x.Status.Name,
            x.CreatedAt,
            x.UpdatedAt
        );
}
