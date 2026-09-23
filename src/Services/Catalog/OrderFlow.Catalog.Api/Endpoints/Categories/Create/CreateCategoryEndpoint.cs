using C = OrderFlow.Catalog.Application.Categories.CategoryFeatures;
using P = OrderFlow.Catalog.Application.Products.ProductFeatures;

namespace OrderFlow.Catalog.Api.Endpoints;

public static partial class CatalogEndpoints
{
    public sealed class CreateCategoryEndpoint(ISender sender)
        : Endpoint<CreateCategoryRequest, C.CategoryResponse>
    {
        public override void Configure()
        {
            Post("/api/categories");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            CreateCategoryRequest request,
            CancellationToken cancellationToken
        ) =>
            await Send.ResponseAsync(
                await sender.Send(
                    new C.CreateCategoryCommand(request.Name, request.Description),
                    cancellationToken
                ),
                201,
                cancellationToken
            );
    }
}
