using MediatR;
using OrderFlow.Payments.Application.Abstractions.Errors;
using OrderFlow.Payments.Application.Abstractions.Messaging;
using OrderFlow.Payments.Application.Abstractions.Payments;
using OrderFlow.Payments.Application.Abstractions.Persistence;
using OrderFlow.Payments.Domain.Payments;

namespace OrderFlow.Payments.Application.Payments;

public static partial class PaymentFeatures
{
    public sealed class CreatePaymentCommandHandler(IPaymentRepository r)
        : IRequestHandler<CreatePaymentCommand, PaymentResponse>
    {
        public async Task<PaymentResponse> Handle(CreatePaymentCommand c, CancellationToken ct)
        {
            if (await r.GetByOrderAsync(c.OrderId, ct) is not null)
                throw new ConflictException("Payment for order already exists.");
            var x = Payment.Create(c.OrderId, c.Amount, PaymentMethod.FromName(c.Method, true));
            await r.AddAsync(x, ct);
            return Map(x);
        }
    }
}
