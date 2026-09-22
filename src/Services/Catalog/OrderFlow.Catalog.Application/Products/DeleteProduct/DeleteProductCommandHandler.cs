using OrderFlow.Catalog.Application.Abstractions.Errors;
using OrderFlow.Catalog.Application.Abstractions.Persistence;

namespace OrderFlow.Catalog.Application.Products;

public static partial class ProductFeatures
{
    public sealed class DeleteProductCommandHandler(
        IProductRepository repository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<DeleteProductCommand>
    {
        public async Task Handle(DeleteProductCommand command, CancellationToken cancellationToken)
        {
            var product = await Find(repository, command.Id, cancellationToken);
            repository.Remove(product);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
