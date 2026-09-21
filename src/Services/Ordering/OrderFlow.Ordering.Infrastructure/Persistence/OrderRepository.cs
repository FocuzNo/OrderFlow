using OrderFlow.Ordering.Application.Abstractions.Persistence;
using OrderFlow.Ordering.Domain.Orders;
using OrderFlow.Ordering.Infrastructure.Persistence.Repositories;

namespace OrderFlow.Ordering.Infrastructure.Persistence;

public sealed class OrderRepository(OrderingDbContext databaseContext)
    : Repository<Order>(databaseContext),
        IOrderRepository
{
    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        DatabaseContext
            .Orders.Include(candidate => candidate.Items)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Order>> GetCustomerOrdersAsync(
        Guid id,
        CancellationToken cancellationToken
    ) =>
        await DatabaseContext
            .Orders.AsNoTracking()
            .Include(candidate => candidate.Items)
            .Where(candidate => candidate.CustomerId == id)
            .OrderByDescending(candidate => candidate.CreatedAt)
            .ToListAsync(cancellationToken);
}
