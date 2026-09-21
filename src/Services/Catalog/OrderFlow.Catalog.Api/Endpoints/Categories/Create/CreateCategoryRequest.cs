using FastEndpoints;
using MediatR;
using C = OrderFlow.Catalog.Application.Categories.CategoryFeatures;
using P = OrderFlow.Catalog.Application.Products.ProductFeatures;

namespace OrderFlow.Catalog.Api.Endpoints;

public static partial class CatalogEndpoints
{
    public sealed class CreateCategoryRequest
    {
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}
