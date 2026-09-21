using OrderFlow.Ordering.Application.Abstractions.Errors;
using OrderFlow.Ordering.Application.Abstractions.Messaging;
using OrderFlow.Ordering.Application.Abstractions.Persistence;
using OrderFlow.Ordering.Domain.Orders;

namespace OrderFlow.Ordering.Application.Orders;

public static partial class OrderFeatures
{
    public sealed class CreateOrderCommandHandler(
        IOrderRepository repository,
        IUnitOfWork unitOfWork
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
            var entity = Order.Create(command.CustomerId, command.CustomerEmail, shippingAddress);
            await repository.AddAsync(entity, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Map(entity);
        }
    }
}
