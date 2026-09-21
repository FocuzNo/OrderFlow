using FastEndpoints;
using MediatR;
using C = OrderFlow.Catalog.Application.Categories.CategoryFeatures;
using P = OrderFlow.Catalog.Application.Products.ProductFeatures;

namespace OrderFlow.Catalog.Api.Endpoints;

public static partial class CatalogEndpoints
{
    public sealed class UpdateCategoryEndpoint(ISender s)
        : Endpoint<UpdateCategoryRequest, C.CategoryResponse>
    {
        public override void Configure()
        {
            Put("/api/categories/{id}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(UpdateCategoryRequest r, CancellationToken ct) =>
            await Send.OkAsync(
                await s.Send(new C.UpdateCategoryCommand(r.Id, r.Name, r.Description), ct),
                ct
            );
    }
}
