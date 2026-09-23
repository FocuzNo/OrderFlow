using C = OrderFlow.Catalog.Application.Categories.CategoryFeatures;
using P = OrderFlow.Catalog.Application.Products.ProductFeatures;

namespace OrderFlow.Catalog.Api.Endpoints;

public static partial class CatalogEndpoints
{
    public sealed record CreateProductRequest
    {
        public string Sku { get; init; } = string.Empty;

        public string Name { get; init; } = string.Empty;

        public string? Description { get; init; }

        public decimal Price { get; init; }

        public Guid CategoryId { get; init; }
    }
}
