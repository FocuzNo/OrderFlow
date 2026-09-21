using FastEndpoints;
using MediatR;
using C = OrderFlow.Catalog.Application.Categories.CategoryFeatures;
using P = OrderFlow.Catalog.Application.Products.ProductFeatures;

namespace OrderFlow.Catalog.Api.Endpoints;

public static partial class CatalogEndpoints
{
    public sealed class DeactivateProductEndpoint(ISender s) : Endpoint<ProductIdRequest>
    {
        public override void Configure()
        {
            Post("/api/products/{id}/deactivate");
            AllowAnonymous();
        }

        public override async Task HandleAsync(ProductIdRequest r, CancellationToken ct)
        {
            await s.Send(new P.DeactivateProductCommand(r.Id), ct);
            await Send.NoContentAsync(ct);
        }
    }
}
