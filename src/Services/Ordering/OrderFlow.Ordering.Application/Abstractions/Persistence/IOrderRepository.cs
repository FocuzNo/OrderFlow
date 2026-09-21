using OrderFlow.Ordering.Domain.Orders;

namespace OrderFlow.Ordering.Application.Abstractions.Persistence;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Order>> GetCustomerOrdersAsync(
        Guid customerId,
        CancellationToken cancellationToken
    );
}
