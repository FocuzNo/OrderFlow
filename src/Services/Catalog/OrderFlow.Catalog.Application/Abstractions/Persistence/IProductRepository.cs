using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Application.Abstractions.Persistence;

public interface IProductRepository
{
    Task AddAsync(Product product, CancellationToken cancellationToken);
}
