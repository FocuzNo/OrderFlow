using OrderFlow.Ordering.Application.Abstractions.Errors;
using OrderFlow.Ordering.Application.Abstractions.Persistence;

namespace OrderFlow.Ordering.Application.Orders;

public static partial class OrderFeatures
{
    public sealed class ConfirmOrderCommandHandler(
        IOrderRepository repository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<ConfirmOrderCommand>
    {
        public async Task Handle(ConfirmOrderCommand command, CancellationToken cancellationToken)
        {
            var order = await Find(repository, command.OrderId, cancellationToken);
            order.Confirm();
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
