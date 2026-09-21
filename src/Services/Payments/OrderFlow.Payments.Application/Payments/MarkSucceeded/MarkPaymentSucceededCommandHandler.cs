using MediatR;
using OrderFlow.Payments.Application.Abstractions.Errors;
using OrderFlow.Payments.Application.Abstractions.Messaging;
using OrderFlow.Payments.Application.Abstractions.Payments;
using OrderFlow.Payments.Application.Abstractions.Persistence;
using OrderFlow.Payments.Domain.Payments;

namespace OrderFlow.Payments.Application.Payments;

public static partial class PaymentFeatures
{
    public sealed class MarkPaymentSucceededCommandHandler(IPaymentRepository r)
        : IRequestHandler<MarkPaymentSucceededCommand>
    {
        public async Task Handle(MarkPaymentSucceededCommand c, CancellationToken ct)
        {
            var x = await Find(r, c.PaymentId, ct);
            x.Succeed(c.Reference);
            await r.SaveAsync(ct);
        }
    }
}
