using OrderFlow.Payments.Application.Abstractions.Errors;
using OrderFlow.Payments.Application.Abstractions.Messaging;
using OrderFlow.Payments.Application.Abstractions.Payments;
using OrderFlow.Payments.Application.Abstractions.Persistence;
using OrderFlow.Payments.Domain.Payments;

namespace OrderFlow.Payments.Application.Payments;

public static partial class PaymentFeatures
{
    public sealed class RefundPaymentCommandHandler(
        IPaymentRepository repository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<RefundPaymentCommand, PaymentResponse>
    {
        public async Task<PaymentResponse> Handle(
            RefundPaymentCommand command,
            CancellationToken cancellationToken
        )
        {
            var entity = await Find(repository, command.PaymentId, cancellationToken);
            entity.Refund(command.Amount, command.Reason);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Map(entity);
        }
    }
}
