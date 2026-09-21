using Microsoft.EntityFrameworkCore;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository(CatalogDbContext db) : IProductRepository
{
    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Products.SingleOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Product>> ListAsync(
        int page,
        int size,
        string? search,
        string? sort,
        CancellationToken ct
    )
    {
        var q = db.Products.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(x => x.Name.Contains(search) || x.Sku.Value.Contains(search));
        q = sort?.ToLowerInvariant() switch
        {
            "price" => q.OrderBy(x => x.Price.Amount),
            "price_desc" => q.OrderByDescending(x => x.Price.Amount),
            "name_desc" => q.OrderByDescending(x => x.Name),
            _ => q.OrderBy(x => x.Name),
        };
        return await q.Skip((page - 1) * size).Take(size).ToListAsync(ct);
    }

    public Task<bool> SkuExistsAsync(string sku, Guid? excluding, CancellationToken ct)
    {
        var v = Sku.Create(sku);
        return db.Products.AnyAsync(
            x => x.Sku == v && (!excluding.HasValue || x.Id != excluding),
            ct
        );
    }

    public async Task AddAsync(Product x, CancellationToken ct)
    {
        db.Products.Add(x);
        await db.SaveChangesAsync(ct);
    }

    public async Task SaveAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
}
