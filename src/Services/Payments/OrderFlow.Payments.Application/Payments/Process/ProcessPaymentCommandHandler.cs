using MediatR;
using OrderFlow.Payments.Application.Abstractions.Errors;
using OrderFlow.Payments.Application.Abstractions.Messaging;
using OrderFlow.Payments.Application.Abstractions.Payments;
using OrderFlow.Payments.Application.Abstractions.Persistence;
using OrderFlow.Payments.Domain.Payments;

namespace OrderFlow.Payments.Application.Payments;

public static partial class PaymentFeatures
{
    public sealed class ProcessPaymentCommandHandler(IPaymentRepository r, IPaymentGateway gateway)
        : IRequestHandler<ProcessPaymentCommand, PaymentResponse>
    {
        public async Task<PaymentResponse> Handle(ProcessPaymentCommand c, CancellationToken ct)
        {
            var x = await Find(r, c.PaymentId, ct);
            x.StartProcessing();
            var result = await gateway.ChargeAsync(x.Id, x.Amount, ct);
            if (result.Succeeded)
                x.Succeed(result.Reference ?? $"DEV-{x.Id:N}");
            else
                x.Fail(result.Error ?? "Payment declined.");
            await r.SaveAsync(ct);
            return Map(x);
        }
    }
}
