using C = OrderFlow.Catalog.Application.Categories.CategoryFeatures;
using P = OrderFlow.Catalog.Application.Products.ProductFeatures;

namespace OrderFlow.Catalog.Api.Endpoints;

public static partial class CatalogEndpoints
{
    public sealed class UpdateCategoryEndpoint(ISender sender)
        : Endpoint<UpdateCategoryRequest, C.CategoryResponse>
    {
        public override void Configure()
        {
            Put("/api/categories/{id}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            UpdateCategoryRequest request,
            CancellationToken cancellationToken
        ) =>
            await Send.OkAsync(
                await sender.Send(
                    new C.UpdateCategoryCommand(request.Id, request.Name, request.Description),
                    cancellationToken
                ),
                cancellationToken
            );
    }
}
