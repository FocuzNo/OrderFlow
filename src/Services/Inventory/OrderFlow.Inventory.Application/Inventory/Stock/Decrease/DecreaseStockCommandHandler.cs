using OrderFlow.Inventory.Application.Abstractions.Errors;
using OrderFlow.Inventory.Application.Abstractions.Messaging;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Stock;
using OrderFlow.Inventory.Domain.Warehouses;

namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class DecreaseStockCommandHandler(
        IInventoryRepository repository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<DecreaseStockCommand, StockResponse>
    {
        public async Task<StockResponse> Handle(
            DecreaseStockCommand command,
            CancellationToken cancellationToken
        )
        {
            var entity = await Find(
                repository,
                command.ProductId,
                command.WarehouseId,
                cancellationToken
            );
            entity.Decrease(command.Quantity);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Map(entity);
        }
    }
}
