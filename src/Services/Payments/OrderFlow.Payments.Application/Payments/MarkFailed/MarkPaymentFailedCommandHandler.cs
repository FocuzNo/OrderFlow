using OrderFlow.Payments.Application.Abstractions.Errors;
using OrderFlow.Payments.Application.Abstractions.Messaging;
using OrderFlow.Payments.Application.Abstractions.Payments;
using OrderFlow.Payments.Application.Abstractions.Persistence;
using OrderFlow.Payments.Domain.Payments;

namespace OrderFlow.Payments.Application.Payments;

public static partial class PaymentFeatures
{
    public sealed class MarkPaymentFailedCommandHandler(
        IPaymentRepository repository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<MarkPaymentFailedCommand>
    {
        public async Task Handle(
            MarkPaymentFailedCommand command,
            CancellationToken cancellationToken
        )
        {
            var entity = await Find(repository, command.PaymentId, cancellationToken);
            entity.Fail(command.Reason);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
