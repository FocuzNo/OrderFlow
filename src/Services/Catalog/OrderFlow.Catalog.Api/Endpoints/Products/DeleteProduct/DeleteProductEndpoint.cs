using F = OrderFlow.Catalog.Application.Products.ProductFeatures;

namespace OrderFlow.Catalog.Api.Endpoints;

public sealed class DeleteProductEndpoint(ISender sender) : Endpoint<DeleteProductRequest>
{
    public override void Configure()
    {
        Delete("/api/products/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(
        DeleteProductRequest request,
        CancellationToken cancellationToken
    )
    {
        await sender.Send(new F.DeleteProductCommand(request.Id), cancellationToken);
        await Send.NoContentAsync(cancellationToken);
    }
}
