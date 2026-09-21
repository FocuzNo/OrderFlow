using FastEndpoints;
using MediatR;
using C = OrderFlow.Catalog.Application.Categories.CategoryFeatures;
using P = OrderFlow.Catalog.Application.Products.ProductFeatures;

namespace OrderFlow.Catalog.Api.Endpoints;

public static partial class CatalogEndpoints
{
    public sealed class GetCategoriesEndpoint(ISender s)
        : EndpointWithoutRequest<IReadOnlyList<C.CategoryResponse>>
    {
        public override void Configure()
        {
            Get("/api/categories");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken ct) =>
            await Send.OkAsync(await s.Send(new C.GetCategoriesQuery(), ct), ct);
    }
}
