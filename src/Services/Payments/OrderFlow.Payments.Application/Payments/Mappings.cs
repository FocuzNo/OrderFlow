using OrderFlow.Payments.Application.Abstractions.Errors;
using OrderFlow.Payments.Application.Abstractions.Messaging;
using OrderFlow.Payments.Application.Abstractions.Payments;
using OrderFlow.Payments.Application.Abstractions.Persistence;
using OrderFlow.Payments.Domain.Payments;

namespace OrderFlow.Payments.Application.Payments;

public static partial class PaymentFeatures
{
    private static async Task<Payment> Find(
        IPaymentRepository repository,
        Guid id,
        CancellationToken cancellationToken
    ) =>
        await repository.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException("Payment was not found.");

    private static PaymentResponse Map(Payment entity) =>
        new(
            entity.Id,
            entity.OrderId,
            entity.Amount,
            entity.Method.Name,
            entity.Status.Name,
            entity.ProviderReference,
            entity.FailureReason,
            entity
                .Refunds.Select(y => new RefundResponse(y.Id, y.Amount, y.Reason, y.CreatedAt))
                .ToArray()
        );
}
