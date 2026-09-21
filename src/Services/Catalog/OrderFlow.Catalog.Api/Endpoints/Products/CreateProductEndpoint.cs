using FastEndpoints;
using MediatR;
using OrderFlow.Catalog.Application.Products.Create;

namespace OrderFlow.Catalog.Api.Endpoints.Products;

public sealed class CreateProductEndpoint(ISender sender)
    : Endpoint<CreateProductRequest, CreateProductResponse>
{
    public override void Configure()
    {
        Post("/api/products");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand(request.Name, request.Description, request.Price);
        var response = await sender.Send(command, cancellationToken);

        await Send.ResponseAsync(response, StatusCodes.Status201Created, cancellationToken);
    }
}
