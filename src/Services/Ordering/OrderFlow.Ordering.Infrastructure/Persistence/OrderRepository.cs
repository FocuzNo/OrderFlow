using Microsoft.EntityFrameworkCore;
using OrderFlow.Ordering.Application.Abstractions.Persistence;
using OrderFlow.Ordering.Domain.Orders;

namespace OrderFlow.Ordering.Infrastructure.Persistence;

public sealed class OrderRepository(OrderingDbContext db) : IOrderRepository
{
    public Task<Order?> GetAsync(Guid id, CancellationToken ct) =>
        db.Orders.Include(x => x.Items).SingleOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Order>> GetCustomerOrdersAsync(Guid id, CancellationToken ct) =>
        await db
            .Orders.AsNoTracking()
            .Include(x => x.Items)
            .Where(x => x.CustomerId == id)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(Order x, CancellationToken ct)
    {
        db.Orders.Add(x);
        await db.SaveChangesAsync(ct);
    }

    public async Task SaveAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
}
