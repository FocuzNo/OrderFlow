using OrderFlow.Catalog.Application.Abstractions.Errors;
using OrderFlow.Catalog.Application.Abstractions.Messaging;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Application.Products;

public static partial class ProductFeatures
{
    public sealed class DeactivateProductCommandHandler(
        IProductRepository repository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<DeactivateProductCommand>
    {
        public async Task Handle(
            DeactivateProductCommand command,
            CancellationToken cancellationToken
        )
        {
            var product = await Find(repository, command.Id, cancellationToken);
            product.Deactivate();
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
