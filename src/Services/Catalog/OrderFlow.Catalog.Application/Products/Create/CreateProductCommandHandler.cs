using MediatR;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Application.Products.Create;

public sealed class CreateProductCommandHandler(IProductRepository productRepository)
    : IRequestHandler<CreateProductCommand, CreateProductResponse>
{
    public async Task<CreateProductResponse> Handle(
        CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        var product = Product.Create(command.Name, command.Description, command.Price);

        await productRepository.AddAsync(product, cancellationToken);

        return new CreateProductResponse(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.CreatedAt,
            product.UpdatedAt);
    }
}
