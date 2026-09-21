namespace OrderFlow.Catalog.Application.Abstractions.Persistence;

public interface IRepository<TEntity>
    where TEntity : class
{
    Task AddAsync(TEntity entity, CancellationToken cancellationToken);
}
