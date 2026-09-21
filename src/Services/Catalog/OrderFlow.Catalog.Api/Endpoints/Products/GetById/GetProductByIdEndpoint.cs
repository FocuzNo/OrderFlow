using FastEndpoints;
using MediatR;
using C = OrderFlow.Catalog.Application.Categories.CategoryFeatures;
using P = OrderFlow.Catalog.Application.Products.ProductFeatures;

namespace OrderFlow.Catalog.Api.Endpoints;

public static partial class CatalogEndpoints
{
    public sealed class GetProductByIdEndpoint(ISender s)
        : Endpoint<ProductIdRequest, P.ProductResponse>
    {
        public override void Configure()
        {
            Get("/api/products/{id}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(ProductIdRequest r, CancellationToken ct) =>
            await Send.OkAsync(await s.Send(new P.GetProductByIdQuery(r.Id), ct), ct);
    }
}
