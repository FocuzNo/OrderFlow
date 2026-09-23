using OrderFlow.Ordering.Application.Abstractions.Errors;
using OrderFlow.Ordering.Application.Abstractions.Messaging;
using OrderFlow.Ordering.Application.Abstractions.Persistence;
using OrderFlow.Ordering.Domain.Orders;

namespace OrderFlow.Ordering.Application.Orders;

public static partial class OrderFeatures
{
    public sealed class AddOrderItemCommandHandler(
        IOrderRepository repository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<AddOrderItemCommand, OrderResponse>
    {
        public async Task<OrderResponse> Handle(
            AddOrderItemCommand command,
            CancellationToken cancellationToken
        )
        {
            var entity = await Find(repository, command.OrderId, cancellationToken);
            entity.AddItem(
                command.ProductId,
                command.ProductName,
                command.UnitPrice,
                command.Quantity
            );
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Map(entity);
        }
    }
}
