using OrderFlow.Ordering.Application.Abstractions.Errors;
using OrderFlow.Ordering.Application.Abstractions.Messaging;
using OrderFlow.Ordering.Application.Abstractions.Persistence;
using OrderFlow.Ordering.Domain.Orders;

namespace OrderFlow.Ordering.Application.Orders;

public static partial class OrderFeatures
{
    public sealed class MarkInventoryReservedCommandHandler(
        IOrderRepository repository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<MarkInventoryReservedCommand>
    {
        public async Task Handle(
            MarkInventoryReservedCommand command,
            CancellationToken cancellationToken
        )
        {
            var entity = await Find(repository, command.OrderId, cancellationToken);
            entity.MarkInventoryReserved();
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
