using C = OrderFlow.Catalog.Application.Categories.CategoryFeatures;
using P = OrderFlow.Catalog.Application.Products.ProductFeatures;

namespace OrderFlow.Catalog.Api.Endpoints;

public static partial class CatalogEndpoints
{
    public sealed class UpdateProductEndpoint(ISender sender)
        : Endpoint<UpdateProductRequest, P.ProductResponse>
    {
        public override void Configure()
        {
            Put("/api/products/{id}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            UpdateProductRequest request,
            CancellationToken cancellationToken
        ) =>
            await Send.OkAsync(
                await sender.Send(
                    new P.UpdateProductCommand(
                        request.Id,
                        request.Name,
                        request.Description,
                        request.CategoryId
                    ),
                    cancellationToken
                ),
                cancellationToken
            );
    }
}
