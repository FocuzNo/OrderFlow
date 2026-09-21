using OrderFlow.Payments.Application.Abstractions.Persistence;

namespace OrderFlow.Payments.Infrastructure.Persistence.Repositories;

public abstract class Repository<TEntity>(PaymentsDbContext databaseContext) : IRepository<TEntity>
    where TEntity : class
{
    protected PaymentsDbContext DatabaseContext { get; } = databaseContext;

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await DatabaseContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    }
}
