using OrderFlow.Catalog.Application.Abstractions.Errors;
using OrderFlow.Catalog.Application.Abstractions.Messaging;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Application.Products;

public static partial class ProductFeatures
{
    private static async Task<Product> Find(
        IProductRepository repository,
        Guid id,
        CancellationToken cancellationToken
    ) =>
        await repository.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException("Product was not found.");
}
