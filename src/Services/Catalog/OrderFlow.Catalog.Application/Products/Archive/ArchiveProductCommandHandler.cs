using OrderFlow.Catalog.Application.Abstractions.Errors;
using OrderFlow.Catalog.Application.Abstractions.Messaging;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Application.Products;

public static partial class ProductFeatures
{
    public sealed class ArchiveProductCommandHandler(
        IProductRepository repository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<ArchiveProductCommand>
    {
        public async Task Handle(ArchiveProductCommand command, CancellationToken cancellationToken)
        {
            var product = await Find(repository, command.Id, cancellationToken);
            product.Archive();
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
