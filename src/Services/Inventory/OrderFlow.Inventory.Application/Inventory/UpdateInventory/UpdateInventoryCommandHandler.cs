using OrderFlow.Inventory.Application.Abstractions.Errors;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Stock;

namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class UpdateInventoryCommandHandler(
        IInventoryRepository repository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<UpdateInventoryCommand, StockResponse>
    {
        public async Task<StockResponse> Handle(
            UpdateInventoryCommand command,
            CancellationToken cancellationToken
        )
        {
            var stock =
                await repository.GetByProductIdAsync(command.ProductId, cancellationToken)
                ?? throw new NotFoundException("Inventory was not found.");
            stock.UpdateQuantity(command.Quantity);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Map(stock);
        }
    }
}
