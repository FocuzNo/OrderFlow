using OrderFlow.Ordering.Application.Abstractions.Persistence;

namespace OrderFlow.Ordering.Infrastructure.Persistence.Repositories;

public abstract class Repository<TEntity>(OrderingDbContext databaseContext) : IRepository<TEntity>
    where TEntity : class
{
    protected OrderingDbContext DatabaseContext { get; } = databaseContext;

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await DatabaseContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    }
}
