using MediatR;
using OrderFlow.Ordering.Application.Abstractions.Errors;
using OrderFlow.Ordering.Application.Abstractions.Messaging;
using OrderFlow.Ordering.Application.Abstractions.Persistence;
using OrderFlow.Ordering.Domain.Orders;

namespace OrderFlow.Ordering.Application.Orders;

public static partial class OrderFeatures
{
    public sealed class ConfirmOrderPaymentCommandHandler(IOrderRepository r)
        : IRequestHandler<ConfirmOrderPaymentCommand>
    {
        public async Task Handle(ConfirmOrderPaymentCommand c, CancellationToken ct)
        {
            var x = await Find(r, c.OrderId, ct);
            x.ConfirmPayment();
            await r.SaveAsync(ct);
        }
    }
}
