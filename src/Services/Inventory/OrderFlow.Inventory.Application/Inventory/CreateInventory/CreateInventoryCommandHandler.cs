using OrderFlow.Inventory.Application.Abstractions.Errors;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Stock;

namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class CreateInventoryCommandHandler(
        IInventoryRepository repository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<CreateInventoryCommand, StockResponse>
    {
        public async Task<StockResponse> Handle(
            CreateInventoryCommand command,
            CancellationToken cancellationToken
        )
        {
            if (
                await repository.GetByProductIdAsync(command.ProductId, cancellationToken)
                is not null
            )
                throw new ConflictException("Inventory already exists for this product.");
            var stock = StockItem.Create(command.ProductId, command.WarehouseId, command.Sku);
            stock.UpdateQuantity(command.Quantity);
            await repository.AddAsync(stock, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Map(stock);
        }
    }
}
