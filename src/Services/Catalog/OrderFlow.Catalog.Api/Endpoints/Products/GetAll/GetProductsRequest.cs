using C = OrderFlow.Catalog.Application.Categories.CategoryFeatures;
using P = OrderFlow.Catalog.Application.Products.ProductFeatures;

namespace OrderFlow.Catalog.Api.Endpoints;

public static partial class CatalogEndpoints
{
    public sealed record GetProductsRequest
    {
        public int Page { get; init; } = 1;

        public int PageSize { get; init; } = 20;

        public string? Search { get; init; }

        public string? Sort { get; init; }
    }
}
