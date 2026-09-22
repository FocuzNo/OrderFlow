using OrderFlow.Inventory.Application.Abstractions.Errors;
using OrderFlow.Inventory.Application.Abstractions.Messaging;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Stock;
using OrderFlow.Inventory.Domain.Warehouses;

namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class CreateWarehouseCommandHandler(
        IInventoryRepository repository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<CreateWarehouseCommand, WarehouseResponse>
    {
        public async Task<WarehouseResponse> Handle(
            CreateWarehouseCommand command,
            CancellationToken cancellationToken
        )
        {
            var entity = Warehouse.Create(command.Name, command.Location);
            await repository.AddAsync(entity, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return new(entity.Id, entity.Name, entity.Location);
        }
    }
}
