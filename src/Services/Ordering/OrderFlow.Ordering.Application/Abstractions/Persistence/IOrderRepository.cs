using OrderFlow.Ordering.Domain.Orders;

namespace OrderFlow.Ordering.Application.Abstractions.Persistence;

public interface IOrderRepository : IRepository<Order>
{
    Task<IReadOnlyList<Order>> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken
    );
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Order>> GetCustomerOrdersAsync(
        Guid customerId,
        CancellationToken cancellationToken
    );
}
