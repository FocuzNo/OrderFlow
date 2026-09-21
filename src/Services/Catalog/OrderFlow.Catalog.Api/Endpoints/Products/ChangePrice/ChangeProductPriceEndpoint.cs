using C = OrderFlow.Catalog.Application.Categories.CategoryFeatures;
using P = OrderFlow.Catalog.Application.Products.ProductFeatures;

namespace OrderFlow.Catalog.Api.Endpoints;

public static partial class CatalogEndpoints
{
    public sealed class ChangeProductPriceEndpoint(ISender sender)
        : Endpoint<ChangeProductPriceRequest, P.ProductResponse>
    {
        public override void Configure()
        {
            Patch("/api/products/{id}/price");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            ChangeProductPriceRequest request,
            CancellationToken cancellationToken
        ) =>
            await Send.OkAsync(
                await sender.Send(
                    new P.ChangeProductPriceCommand(request.Id, request.Price),
                    cancellationToken
                ),
                cancellationToken
            );
    }
}
