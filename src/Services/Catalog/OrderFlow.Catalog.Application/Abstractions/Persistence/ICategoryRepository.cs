using OrderFlow.Catalog.Domain.Categories;

namespace OrderFlow.Catalog.Application.Abstractions.Persistence;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Category>> ListAsync(CancellationToken cancellationToken);
    Task<bool> NameExistsAsync(string name, Guid? excludingId, CancellationToken cancellationToken);
}
