using OrderFlow.Ordering.Domain.Orders;

namespace OrderFlow.Ordering.Application.Abstractions.Messaging;

public interface IOrderCreatedOutboxWriter
{
    void Add(Order order);
}
