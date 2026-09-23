using OrderFlow.Inventory.Application.Abstractions.Errors;
using OrderFlow.Inventory.Application.Abstractions.Messaging;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Stock;
using OrderFlow.Inventory.Domain.Warehouses;

namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class CreateStockItemCommandHandler(
        IInventoryRepository repository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<CreateStockItemCommand, StockResponse>
    {
        public async Task<StockResponse> Handle(
            CreateStockItemCommand command,
            CancellationToken cancellationToken
        )
        {
            if (
                await repository.GetStockAsync(
                    command.ProductId,
                    command.WarehouseId,
                    cancellationToken
                )
                is not null
            )
                throw new ConflictException("Stock item already exists.");
            var entity = StockItem.Create(command.ProductId, command.WarehouseId, command.Sku);
            await repository.AddAsync(entity, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Map(entity);
        }
    }
}
