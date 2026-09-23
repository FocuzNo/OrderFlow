using OrderFlow.Catalog.Application.Abstractions.Errors;
using OrderFlow.Catalog.Application.Abstractions.Messaging;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Application.Products;

public static partial class ProductFeatures
{
    public sealed class CreateProductCommandHandler(
        IProductRepository products,
        ICategoryRepository categories,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<CreateProductCommand, ProductResponse>
    {
        public async Task<ProductResponse> Handle(
            CreateProductCommand command,
            CancellationToken cancellationToken
        )
        {
            if (await products.SkuExistsAsync(command.Sku, null, cancellationToken))
                throw new ConflictException("SKU already exists.");
            if (await categories.GetByIdAsync(command.CategoryId, cancellationToken) is null)
                throw new NotFoundException("Category was not found.");
            var product = Product.Create(
                command.Sku,
                command.Name,
                command.Description,
                command.Price,
                command.CategoryId
            );
            await products.AddAsync(product, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return ProductResponse.From(product);
        }
    }
}
