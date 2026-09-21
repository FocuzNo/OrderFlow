using MediatR;
using OrderFlow.Ordering.Application.Abstractions.Errors;
using OrderFlow.Ordering.Application.Abstractions.Messaging;
using OrderFlow.Ordering.Application.Abstractions.Persistence;
using OrderFlow.Ordering.Domain.Orders;

namespace OrderFlow.Ordering.Application.Orders;

public static partial class OrderFeatures
{
    public sealed class MarkInventoryReservedCommandHandler(IOrderRepository r)
        : IRequestHandler<MarkInventoryReservedCommand>
    {
        public async Task Handle(MarkInventoryReservedCommand c, CancellationToken ct)
        {
            var x = await Find(r, c.OrderId, ct);
            x.MarkInventoryReserved();
            await r.SaveAsync(ct);
        }
    }
}
