using OrderFlow.Catalog.Application.Abstractions.Errors;
using OrderFlow.Catalog.Application.Abstractions.Messaging;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Application.Products;

public static partial class ProductFeatures
{
    public sealed class ActivateProductCommandHandler(
        IProductRepository repository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<ActivateProductCommand>
    {
        public async Task Handle(
            ActivateProductCommand command,
            CancellationToken cancellationToken
        )
        {
            var product = await Find(repository, command.Id, cancellationToken);
            product.Activate();
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
