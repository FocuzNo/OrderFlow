using C = OrderFlow.Catalog.Application.Categories.CategoryFeatures;
using P = OrderFlow.Catalog.Application.Products.ProductFeatures;

namespace OrderFlow.Catalog.Api.Endpoints;

public static partial class CatalogEndpoints
{
    public sealed class GetProductByIdEndpoint(ISender sender)
        : Endpoint<ProductIdRequest, P.ProductResponse>
    {
        public override void Configure()
        {
            Get("/api/products/{id}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            ProductIdRequest request,
            CancellationToken cancellationToken
        ) =>
            await Send.OkAsync(
                await sender.Send(new P.GetProductByIdQuery(request.Id), cancellationToken),
                cancellationToken
            );
    }
}
