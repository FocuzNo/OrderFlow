using OrderFlow.Inventory.Application.Abstractions.Persistence;

namespace OrderFlow.Inventory.Infrastructure.Persistence.Repositories;

public abstract class Repository<TEntity>(InventoryDbContext databaseContext) : IRepository<TEntity>
    where TEntity : class
{
    protected InventoryDbContext DatabaseContext { get; } = databaseContext;

    public void Remove(TEntity entity) => DatabaseContext.Set<TEntity>().Remove(entity);

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await DatabaseContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    }
}
