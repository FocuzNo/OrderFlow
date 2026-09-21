using FastEndpoints;
using MediatR;
using C = OrderFlow.Catalog.Application.Categories.CategoryFeatures;
using P = OrderFlow.Catalog.Application.Products.ProductFeatures;

namespace OrderFlow.Catalog.Api.Endpoints;

public static partial class CatalogEndpoints
{
    public sealed class ChangeProductPriceEndpoint(ISender s)
        : Endpoint<ChangeProductPriceRequest, P.ProductResponse>
    {
        public override void Configure()
        {
            Patch("/api/products/{id}/price");
            AllowAnonymous();
        }

        public override async Task HandleAsync(ChangeProductPriceRequest r, CancellationToken ct) =>
            await Send.OkAsync(
                await s.Send(new P.ChangeProductPriceCommand(r.Id, r.Price), ct),
                ct
            );
    }
}
