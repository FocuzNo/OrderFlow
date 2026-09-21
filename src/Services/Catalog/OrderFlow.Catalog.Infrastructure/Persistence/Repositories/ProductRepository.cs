using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository(CatalogDbContext databaseContext)
    : Repository<Product>(databaseContext),
        IProductRepository
{
    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        DatabaseContext.Products.SingleOrDefaultAsync(
            candidate => candidate.Id == id,
            cancellationToken
        );

    public async Task<IReadOnlyList<Product>> ListAsync(
        int page,
        int size,
        string? search,
        string? sort,
        CancellationToken cancellationToken
    )
    {
        var productsQuery = DatabaseContext.Products.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            productsQuery = productsQuery.Where(candidate =>
                candidate.Name.Contains(search) || candidate.Sku.Value.Contains(search)
            );
        productsQuery = sort?.ToLowerInvariant() switch
        {
            "price" => productsQuery.OrderBy(candidate => candidate.Price.Amount),
            "price_desc" => productsQuery.OrderByDescending(candidate => candidate.Price.Amount),
            "name_desc" => productsQuery.OrderByDescending(candidate => candidate.Name),
            _ => productsQuery.OrderBy(candidate => candidate.Name),
        };
        return await productsQuery
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> SkuExistsAsync(
        string sku,
        Guid? excluding,
        CancellationToken cancellationToken
    )
    {
        var skuValue = Sku.Create(sku);
        return DatabaseContext.Products.AnyAsync(
            candidate =>
                candidate.Sku == skuValue && (!excluding.HasValue || candidate.Id != excluding),
            cancellationToken
        );
    }
}
