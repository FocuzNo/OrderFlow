using MediatR;
using OrderFlow.Payments.Application.Abstractions.Errors;
using OrderFlow.Payments.Application.Abstractions.Messaging;
using OrderFlow.Payments.Application.Abstractions.Payments;
using OrderFlow.Payments.Application.Abstractions.Persistence;
using OrderFlow.Payments.Domain.Payments;

namespace OrderFlow.Payments.Application.Payments;

public static partial class PaymentFeatures
{
    public sealed class RefundPaymentCommandHandler(IPaymentRepository r)
        : IRequestHandler<RefundPaymentCommand, PaymentResponse>
    {
        public async Task<PaymentResponse> Handle(RefundPaymentCommand c, CancellationToken ct)
        {
            var x = await Find(r, c.PaymentId, ct);
            x.Refund(c.Amount, c.Reason);
            await r.SaveAsync(ct);
            return Map(x);
        }
    }
}
