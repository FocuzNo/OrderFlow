using OrderFlow.Inventory.Application.Abstractions.Errors;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Stock;

namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class DeleteInventoryCommandHandler(
        IInventoryRepository repository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<DeleteInventoryCommand>
    {
        public async Task Handle(
            DeleteInventoryCommand command,
            CancellationToken cancellationToken
        )
        {
            var stock =
                await repository.GetByProductIdAsync(command.ProductId, cancellationToken)
                ?? throw new NotFoundException("Inventory was not found.");
            stock.EnsureCanDelete();
            repository.Remove(stock);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
