using FastEndpoints;
using MediatR;
using C = OrderFlow.Catalog.Application.Categories.CategoryFeatures;
using P = OrderFlow.Catalog.Application.Products.ProductFeatures;

namespace OrderFlow.Catalog.Api.Endpoints;

public static partial class CatalogEndpoints
{
    public sealed class GetProductsEndpoint(ISender s)
        : Endpoint<GetProductsRequest, IReadOnlyList<P.ProductResponse>>
    {
        public override void Configure()
        {
            Get("/api/products");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GetProductsRequest r, CancellationToken ct) =>
            await Send.OkAsync(
                await s.Send(new P.GetProductsQuery(r.Page, r.PageSize, r.Search, r.Sort), ct),
                ct
            );
    }
}
