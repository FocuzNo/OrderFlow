using OrderFlow.Ordering.Application.Abstractions.Errors;
using OrderFlow.Ordering.Application.Abstractions.Persistence;

namespace OrderFlow.Ordering.Application.Orders;

public static partial class OrderFeatures
{
    public sealed class MarkPaymentProcessingCommandHandler(
        IOrderRepository repository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<MarkPaymentProcessingCommand>
    {
        public async Task Handle(
            MarkPaymentProcessingCommand command,
            CancellationToken cancellationToken
        )
        {
            var order = await Find(repository, command.OrderId, cancellationToken);
            order.MarkPaymentProcessing();
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
