using OrderFlow.Ordering.Application.Abstractions.Persistence;
using OrderFlow.Ordering.Domain.Orders;
using OrderFlow.Ordering.Application.Abstractions.Messaging;

namespace OrderFlow.Ordering.Application.Orders;

public static partial class OrderFeatures
{
    public sealed class CreateOrderCommandHandler(
        IOrderRepository repository,
        IUnitOfWork unitOfWork,
        IOrderCreatedOutboxWriter outboxWriter
    ) : IRequestHandler<CreateOrderCommand, OrderResponse>
    {
        public async Task<OrderResponse> Handle(
            CreateOrderCommand command,
            CancellationToken cancellationToken
        )
        {
            var shippingAddress = ShippingAddress.Create(
                command.ShippingAddress.Line1,
                command.ShippingAddress.City,
                command.ShippingAddress.PostalCode,
                command.ShippingAddress.Country
            );

            var items = command.Items
                .Select(item =>
                    OrderItem.Create(
                        item.ProductId,
                        item.ProductName,
                        item.UnitPrice,
                        item.Quantity
                    )
                )
                .ToArray();

            var entity = Order.Create(
                command.CustomerId,
                command.CustomerEmail,
                shippingAddress,
                items
            );

            await repository.AddAsync(
                entity,
                cancellationToken
            );

            outboxWriter.Add(
                entity
            );

            await unitOfWork.SaveChangesAsync(
                cancellationToken
            );

            return Map(entity);
        }
    }
}
