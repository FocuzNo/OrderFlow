using OrderFlow.Catalog.Application.Abstractions.Errors;
using OrderFlow.Catalog.Application.Abstractions.Messaging;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Application.Products;

public static partial class ProductFeatures
{
    public sealed record CreateProductCommand(
        string Sku,
        string Name,
        string? Description,
        decimal Price,
        Guid CategoryId
    ) : ICommand<ProductResponse>;
}
