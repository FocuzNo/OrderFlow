using OrderFlow.Payments.Application.Abstractions.Errors;
using OrderFlow.Payments.Application.Abstractions.Messaging;
using OrderFlow.Payments.Application.Abstractions.Payments;
using OrderFlow.Payments.Application.Abstractions.Persistence;
using OrderFlow.Payments.Domain.Payments;

namespace OrderFlow.Payments.Application.Payments;

public static partial class PaymentFeatures
{
    public sealed class MarkPaymentSucceededCommandHandler(
        IPaymentRepository repository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<MarkPaymentSucceededCommand>
    {
        public async Task Handle(
            MarkPaymentSucceededCommand command,
            CancellationToken cancellationToken
        )
        {
            var entity = await Find(repository, command.PaymentId, cancellationToken);
            entity.Succeed(command.Reference);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
