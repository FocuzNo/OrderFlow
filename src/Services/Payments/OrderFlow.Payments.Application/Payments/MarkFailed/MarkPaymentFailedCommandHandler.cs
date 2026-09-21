using MediatR;
using OrderFlow.Payments.Application.Abstractions.Errors;
using OrderFlow.Payments.Application.Abstractions.Messaging;
using OrderFlow.Payments.Application.Abstractions.Payments;
using OrderFlow.Payments.Application.Abstractions.Persistence;
using OrderFlow.Payments.Domain.Payments;

namespace OrderFlow.Payments.Application.Payments;

public static partial class PaymentFeatures
{
    public sealed class MarkPaymentFailedCommandHandler(IPaymentRepository r)
        : IRequestHandler<MarkPaymentFailedCommand>
    {
        public async Task Handle(MarkPaymentFailedCommand c, CancellationToken ct)
        {
            var x = await Find(r, c.PaymentId, ct);
            x.Fail(c.Reason);
            await r.SaveAsync(ct);
        }
    }
}
