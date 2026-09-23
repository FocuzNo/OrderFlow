using OrderFlow.Ordering.Application.Abstractions.Errors;
using OrderFlow.Ordering.Application.Abstractions.Messaging;
using OrderFlow.Ordering.Application.Abstractions.Persistence;
using OrderFlow.Ordering.Domain.Orders;

namespace OrderFlow.Ordering.Application.Orders;

public static partial class OrderFeatures
{
    public sealed class RemoveOrderItemCommandHandler(
        IOrderRepository repository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<RemoveOrderItemCommand, OrderResponse>
    {
        public async Task<OrderResponse> Handle(
            RemoveOrderItemCommand command,
            CancellationToken cancellationToken
        )
        {
            var entity = await Find(repository, command.OrderId, cancellationToken);
            entity.RemoveItem(command.ItemId);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Map(entity);
        }
    }
}
