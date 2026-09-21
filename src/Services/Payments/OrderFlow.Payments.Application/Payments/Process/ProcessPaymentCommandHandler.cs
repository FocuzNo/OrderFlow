using OrderFlow.Payments.Application.Abstractions.Errors;
using OrderFlow.Payments.Application.Abstractions.Messaging;
using OrderFlow.Payments.Application.Abstractions.Payments;
using OrderFlow.Payments.Application.Abstractions.Persistence;
using OrderFlow.Payments.Domain.Payments;

namespace OrderFlow.Payments.Application.Payments;

public static partial class PaymentFeatures
{
    public sealed class ProcessPaymentCommandHandler(
        IPaymentRepository repository,
        IPaymentGateway gateway,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<ProcessPaymentCommand, PaymentResponse>
    {
        public async Task<PaymentResponse> Handle(
            ProcessPaymentCommand command,
            CancellationToken cancellationToken
        )
        {
            var entity = await Find(repository, command.PaymentId, cancellationToken);
            entity.StartProcessing();
            var result = await gateway.ChargeAsync(entity.Id, entity.Amount, cancellationToken);
            if (result.Succeeded)
                entity.Succeed(result.Reference ?? $"DEV-{entity.Id:N}");
            else
                entity.Fail(result.Error ?? "Payment declined.");
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Map(entity);
        }
    }
}
