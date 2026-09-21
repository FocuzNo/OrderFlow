using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Application.Abstractions.Persistence;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Product>> ListAsync(
        int page,
        int pageSize,
        string? search,
        string? sort,
        CancellationToken cancellationToken
    );
    Task<bool> SkuExistsAsync(string sku, Guid? excludingId, CancellationToken cancellationToken);
    Task AddAsync(Product product, CancellationToken cancellationToken);
    Task SaveAsync(CancellationToken cancellationToken);
}
