using C = OrderFlow.Catalog.Application.Categories.CategoryFeatures;
using P = OrderFlow.Catalog.Application.Products.ProductFeatures;

namespace OrderFlow.Catalog.Api.Endpoints;

public static partial class CatalogEndpoints
{
    public sealed class CreateProductEndpoint(ISender sender)
        : Endpoint<CreateProductRequest, P.ProductResponse>
    {
        public override void Configure()
        {
            Post("/api/products");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            CreateProductRequest request,
            CancellationToken cancellationToken
        ) =>
            await Send.ResponseAsync(
                await sender.Send(
                    new P.CreateProductCommand(
                        request.Sku,
                        request.Name,
                        request.Description,
                        request.Price,
                        request.CategoryId
                    ),
                    cancellationToken
                ),
                201,
                cancellationToken
            );
    }
}
