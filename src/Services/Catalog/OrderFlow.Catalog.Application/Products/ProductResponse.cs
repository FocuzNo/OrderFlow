using OrderFlow.Catalog.Application.Abstractions.Errors;
using OrderFlow.Catalog.Application.Abstractions.Messaging;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Application.Products;

public static partial class ProductFeatures
{
    public sealed record ProductResponse(
        Guid Id,
        string Sku,
        string Name,
        string? Description,
        decimal Price,
        Guid CategoryId,
        string Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt
    )
    {
        public static ProductResponse From(Product candidate) =>
            new(
                candidate.Id,
                candidate.Sku.Value,
                candidate.Name,
                candidate.Description,
                candidate.Price.Amount,
                candidate.CategoryId,
                candidate.Status.Name,
                candidate.CreatedAt,
                candidate.UpdatedAt
            );
    }
}
