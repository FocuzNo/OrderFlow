using C = OrderFlow.Catalog.Application.Categories.CategoryFeatures;
using P = OrderFlow.Catalog.Application.Products.ProductFeatures;

namespace OrderFlow.Catalog.Api.Endpoints;

public static partial class CatalogEndpoints
{
    public sealed class ActivateProductEndpoint(ISender sender) : Endpoint<ProductIdRequest>
    {
        public override void Configure()
        {
            Post("/api/products/{id}/activate");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            ProductIdRequest request,
            CancellationToken cancellationToken
        )
        {
            await sender.Send(new P.ActivateProductCommand(request.Id), cancellationToken);
            await Send.NoContentAsync(cancellationToken);
        }
    }
}
