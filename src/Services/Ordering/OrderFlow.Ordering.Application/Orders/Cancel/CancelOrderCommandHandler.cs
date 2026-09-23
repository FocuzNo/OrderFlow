using OrderFlow.Ordering.Application.Abstractions.Errors;
using OrderFlow.Ordering.Application.Abstractions.Messaging;
using OrderFlow.Ordering.Application.Abstractions.Persistence;
using OrderFlow.Ordering.Domain.Orders;

namespace OrderFlow.Ordering.Application.Orders;

public static partial class OrderFeatures
{
    public sealed class CancelOrderCommandHandler(
        IOrderRepository repository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<CancelOrderCommand, OrderResponse>
    {
        public async Task<OrderResponse> Handle(
            CancelOrderCommand command,
            CancellationToken cancellationToken
        )
        {
            var entity = await Find(repository, command.OrderId, cancellationToken);
            entity.Cancel(command.Reason);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Map(entity);
        }
    }
}
