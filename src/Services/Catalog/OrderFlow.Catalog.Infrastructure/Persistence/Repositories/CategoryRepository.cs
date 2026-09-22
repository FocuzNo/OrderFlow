using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Categories;

namespace OrderFlow.Catalog.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository(CatalogDbContext databaseContext)
    : Repository<Category>(databaseContext),
        ICategoryRepository
{
    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        DatabaseContext.Categories.SingleOrDefaultAsync(
            candidate => candidate.Id == id,
            cancellationToken
        );

    public async Task<IReadOnlyList<Category>> ListAsync(CancellationToken cancellationToken) =>
        await DatabaseContext
            .Categories.AsNoTracking()
            .OrderBy(candidate => candidate.Name)
            .ToListAsync(cancellationToken);

    public Task<bool> NameExistsAsync(
        string name,
        Guid? excluding,
        CancellationToken cancellationToken
    ) =>
        DatabaseContext.Categories.AnyAsync(
            candidate =>
                candidate.Name == name && (!excluding.HasValue || candidate.Id != excluding),
            cancellationToken
        );
}
