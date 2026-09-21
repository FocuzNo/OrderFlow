using FastEndpoints;
using MediatR;
using C = OrderFlow.Catalog.Application.Categories.CategoryFeatures;
using P = OrderFlow.Catalog.Application.Products.ProductFeatures;

namespace OrderFlow.Catalog.Api.Endpoints;

public static partial class CatalogEndpoints
{
    public sealed class CreateProductEndpoint(ISender s)
        : Endpoint<CreateProductRequest, P.ProductResponse>
    {
        public override void Configure()
        {
            Post("/api/products");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CreateProductRequest r, CancellationToken ct) =>
            await Send.ResponseAsync(
                await s.Send(
                    new P.CreateProductCommand(r.Sku, r.Name, r.Description, r.Price, r.CategoryId),
                    ct
                ),
                201,
                ct
            );
    }
}
