using OrderFlow.Catalog.Application.Abstractions.Errors;
using OrderFlow.Catalog.Application.Abstractions.Messaging;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Application.Products;

public static partial class ProductFeatures
{
    public sealed class UpdateProductCommandHandler(
        IProductRepository products,
        ICategoryRepository categories,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<UpdateProductCommand, ProductResponse>
    {
        public async Task<ProductResponse> Handle(
            UpdateProductCommand command,
            CancellationToken cancellationToken
        )
        {
            var product = await Find(products, command.Id, cancellationToken);
            if (await categories.GetByIdAsync(command.CategoryId, cancellationToken) is null)
                throw new NotFoundException("Category was not found.");
            product.Update(command.Name, command.Description, command.CategoryId);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return ProductResponse.From(product);
        }
    }
}
