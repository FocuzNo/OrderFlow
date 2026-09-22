using C = OrderFlow.Catalog.Application.Categories.CategoryFeatures;
using P = OrderFlow.Catalog.Application.Products.ProductFeatures;

namespace OrderFlow.Catalog.Api.Endpoints;

public static partial class CatalogEndpoints
{
    public sealed record CreateCategoryRequest
    {
        public string Name { get; init; } = string.Empty;

        public string? Description { get; init; }
    }
}
