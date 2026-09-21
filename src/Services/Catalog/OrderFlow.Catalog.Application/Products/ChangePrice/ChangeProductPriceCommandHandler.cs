using OrderFlow.Catalog.Application.Abstractions.Errors;
using OrderFlow.Catalog.Application.Abstractions.Messaging;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Application.Products;

public static partial class ProductFeatures
{
    public sealed class ChangeProductPriceCommandHandler(
        IProductRepository repository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<ChangeProductPriceCommand, ProductResponse>
    {
        public async Task<ProductResponse> Handle(
            ChangeProductPriceCommand command,
            CancellationToken cancellationToken
        )
        {
            var product = await Find(repository, command.Id, cancellationToken);
            product.ChangePrice(command.Price);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return ProductResponse.From(product);
        }
    }
}
