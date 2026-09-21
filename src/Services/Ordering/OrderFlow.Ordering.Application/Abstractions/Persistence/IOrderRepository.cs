using OrderFlow.Ordering.Domain.Orders;

namespace OrderFlow.Ordering.Application.Abstractions.Persistence;

public interface IOrderRepository
{
    Task<Order?> GetAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Order>> GetCustomerOrdersAsync(Guid customerId, CancellationToken ct);
    Task AddAsync(Order order, CancellationToken ct);
    Task SaveAsync(CancellationToken ct);
}
