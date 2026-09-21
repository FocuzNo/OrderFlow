using FastEndpoints;
using MediatR;
using C = OrderFlow.Catalog.Application.Categories.CategoryFeatures;
using P = OrderFlow.Catalog.Application.Products.ProductFeatures;

namespace OrderFlow.Catalog.Api.Endpoints;

public static partial class CatalogEndpoints
{
    public sealed class CreateCategoryEndpoint(ISender s)
        : Endpoint<CreateCategoryRequest, C.CategoryResponse>
    {
        public override void Configure()
        {
            Post("/api/categories");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CreateCategoryRequest r, CancellationToken ct) =>
            await Send.ResponseAsync(
                await s.Send(new C.CreateCategoryCommand(r.Name, r.Description), ct),
                201,
                ct
            );
    }
}
