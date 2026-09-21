using OrderFlow.Ordering.Application.Abstractions.Errors;
using OrderFlow.Ordering.Application.Abstractions.Messaging;
using OrderFlow.Ordering.Application.Abstractions.Persistence;
using OrderFlow.Ordering.Domain.Orders;

namespace OrderFlow.Ordering.Application.Orders;

public static partial class OrderFeatures
{
    private static async Task<Order> Find(
        IOrderRepository repository,
        Guid id,
        CancellationToken cancellationToken
    ) =>
        await repository.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException("Order was not found.");

    private static OrderResponse Map(Order entity) =>
        new(
            entity.Id,
            entity.CustomerId,
            entity.CustomerEmail.Value,
            new(
                entity.ShippingAddress.Line1,
                entity.ShippingAddress.City,
                entity.ShippingAddress.PostalCode,
                entity.ShippingAddress.Country
            ),
            entity
                .Items.Select(orderItem => new OrderItemResponse(
                    orderItem.Id,
                    orderItem.ProductId,
                    orderItem.ProductName,
                    orderItem.UnitPrice,
                    orderItem.Quantity,
                    orderItem.Total
                ))
                .ToArray(),
            entity.TotalAmount,
            entity.Status.Name,
            entity.CreatedAt,
            entity.UpdatedAt
        );
}
