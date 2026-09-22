using OrderFlow.Payments.Application.Abstractions.Errors;
using OrderFlow.Payments.Application.Abstractions.Messaging;
using OrderFlow.Payments.Application.Abstractions.Payments;
using OrderFlow.Payments.Application.Abstractions.Persistence;
using OrderFlow.Payments.Domain.Payments;

namespace OrderFlow.Payments.Application.Payments;

public static partial class PaymentFeatures
{
    public sealed class CreatePaymentCommandHandler(
        IPaymentRepository repository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<CreatePaymentCommand, PaymentResponse>
    {
        public async Task<PaymentResponse> Handle(
            CreatePaymentCommand command,
            CancellationToken cancellationToken
        )
        {
            if (await repository.GetByOrderAsync(command.OrderId, cancellationToken) is not null)
                throw new ConflictException("Payment for order already exists.");
            var entity = Payment.Create(
                command.OrderId,
                command.Amount,
                PaymentMethod.FromName(command.Method, true)
            );
            entity.StartProcessing();
            if (command.SimulateFailure)
                entity.Fail("Simulated payment failure.");
            else
                entity.Succeed($"SIM-{entity.Id:N}");
            await repository.AddAsync(entity, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Map(entity);
        }
    }
}
