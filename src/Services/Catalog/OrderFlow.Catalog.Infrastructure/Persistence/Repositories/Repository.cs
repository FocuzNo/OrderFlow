using OrderFlow.Catalog.Application.Abstractions.Persistence;

namespace OrderFlow.Catalog.Infrastructure.Persistence.Repositories;

public abstract class Repository<TEntity>(CatalogDbContext databaseContext) : IRepository<TEntity>
    where TEntity : class
{
    protected CatalogDbContext DatabaseContext { get; } = databaseContext;

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await DatabaseContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    }
}
