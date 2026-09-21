using OrderFlow.Catalog.Domain.Categories;

namespace OrderFlow.Catalog.Application.Abstractions.Persistence;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Category>> ListAsync(CancellationToken cancellationToken);
    Task<bool> NameExistsAsync(string name, Guid? excludingId, CancellationToken cancellationToken);
    Task AddAsync(Category category, CancellationToken cancellationToken);
    Task SaveAsync(CancellationToken cancellationToken);
}
