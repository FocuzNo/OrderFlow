using MediatR;
using OrderFlow.Ordering.Application.Abstractions.Errors;
using OrderFlow.Ordering.Application.Abstractions.Messaging;
using OrderFlow.Ordering.Application.Abstractions.Persistence;
using OrderFlow.Ordering.Domain.Orders;

namespace OrderFlow.Ordering.Application.Orders;

public static partial class OrderFeatures
{
    public sealed class FailOrderWorkflowCommandHandler(IOrderRepository r)
        : IRequestHandler<FailOrderWorkflowCommand>
    {
        public async Task Handle(FailOrderWorkflowCommand c, CancellationToken ct)
        {
            var x = await Find(r, c.OrderId, ct);
            x.Cancel(c.Reason);
            await r.SaveAsync(ct);
        }
    }
}
