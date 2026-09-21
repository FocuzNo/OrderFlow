using OrderFlow.Inventory.Application.Abstractions.Persistence;

namespace OrderFlow.Inventory.Infrastructure.Persistence.Repositories;

public abstract class Repository<TEntity>(InventoryDbContext databaseContext) : IRepository<TEntity>
    where TEntity : class
{
    protected InventoryDbContext DatabaseContext { get; } = databaseContext;

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await DatabaseContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    }
}
