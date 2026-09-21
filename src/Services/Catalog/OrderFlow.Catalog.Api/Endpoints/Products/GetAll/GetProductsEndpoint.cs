using C = OrderFlow.Catalog.Application.Categories.CategoryFeatures;
using P = OrderFlow.Catalog.Application.Products.ProductFeatures;

namespace OrderFlow.Catalog.Api.Endpoints;

public static partial class CatalogEndpoints
{
    public sealed class GetProductsEndpoint(ISender sender)
        : Endpoint<GetProductsRequest, IReadOnlyList<P.ProductResponse>>
    {
        public override void Configure()
        {
            Get("/api/products");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            GetProductsRequest request,
            CancellationToken cancellationToken
        ) =>
            await Send.OkAsync(
                await sender.Send(
                    new P.GetProductsQuery(
                        request.Page,
                        request.PageSize,
                        request.Search,
                        request.Sort
                    ),
                    cancellationToken
                ),
                cancellationToken
            );
    }
}
