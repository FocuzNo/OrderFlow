using OrderFlow.Notifications.Application.Abstractions.Persistence;

namespace OrderFlow.Notifications.Infrastructure.Persistence.Repositories;

public abstract class Repository<TEntity>(NotificationsDbContext databaseContext)
    : IRepository<TEntity>
    where TEntity : class
{
    protected NotificationsDbContext DatabaseContext { get; } = databaseContext;

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await DatabaseContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    }
}
