using FastEndpoints;
using MediatR;
using OrderFlow.Catalog.Application.Products;

namespace OrderFlow.Catalog.Api.Endpoints.Products;

public class ProductIdRequest { public Guid Id { get; set; } }
public sealed class UpdateProductRequest : ProductIdRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
}

public sealed class GetProductEndpoint(ISender sender) : Endpoint<ProductIdRequest, ProductDto>
{
    public override void Configure() { Get("/api/products/{id}"); AllowAnonymous(); }
    public override async Task HandleAsync(ProductIdRequest request, CancellationToken ct) =>
        await Send.OkAsync(await sender.Send(new GetProductQuery(request.Id), ct), ct);
}

public sealed class ListProductsEndpoint(ISender sender) : EndpointWithoutRequest<IReadOnlyList<ProductDto>>
{
    public override void Configure() { Get("/api/products"); AllowAnonymous(); }
    public override async Task HandleAsync(CancellationToken ct) =>
        await Send.OkAsync(await sender.Send(new ListProductsQuery(), ct), ct);
}

public sealed class UpdateProductEndpoint(ISender sender) : Endpoint<UpdateProductRequest, ProductDto>
{
    public override void Configure() { Put("/api/products/{id}"); AllowAnonymous(); }
    public override async Task HandleAsync(UpdateProductRequest request, CancellationToken ct) =>
        await Send.OkAsync(await sender.Send(new UpdateProductCommand(request.Id, request.Name, request.Description, request.Price), ct), ct);
}

public sealed class DeleteProductEndpoint(ISender sender) : Endpoint<ProductIdRequest>
{
    public override void Configure() { Delete("/api/products/{id}"); AllowAnonymous(); }
    public override async Task HandleAsync(ProductIdRequest request, CancellationToken ct)
    {
        await sender.Send(new DeleteProductCommand(request.Id), ct);
        await Send.NoContentAsync(ct);
    }
}
