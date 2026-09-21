using MediatR;
using OrderFlow.Payments.Application.Abstractions.Errors;
using OrderFlow.Payments.Application.Abstractions.Messaging;
using OrderFlow.Payments.Application.Abstractions.Payments;
using OrderFlow.Payments.Application.Abstractions.Persistence;
using OrderFlow.Payments.Domain.Payments;

namespace OrderFlow.Payments.Application.Payments;

public static partial class PaymentFeatures
{
    private static async Task<Payment> Find(IPaymentRepository r, Guid id, CancellationToken ct) =>
        await r.GetAsync(id, ct) ?? throw new NotFoundException("Payment was not found.");

    private static PaymentResponse Map(Payment x) =>
        new(
            x.Id,
            x.OrderId,
            x.Amount,
            x.Method.Name,
            x.Status.Name,
            x.ProviderReference,
            x.FailureReason,
            x.Refunds.Select(y => new RefundResponse(y.Id, y.Amount, y.Reason, y.CreatedAt))
                .ToArray()
        );
}
