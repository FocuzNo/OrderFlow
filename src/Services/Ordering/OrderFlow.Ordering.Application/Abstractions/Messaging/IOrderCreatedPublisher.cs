using OrderFlow.Ordering.Domain.Orders;

namespace OrderFlow.Ordering.Application.Abstractions.Messaging;

public interface IOrderCreatedPublisher
{
    Task PublishAsync(Order order, CancellationToken cancellationToken);
}
