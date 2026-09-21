using OrderFlow.Inventory.Application.Abstractions.Errors;
using OrderFlow.Inventory.Application.Abstractions.Messaging;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Stock;
using OrderFlow.Inventory.Domain.Warehouses;

namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class ReserveInventoryCommandHandler(
        IInventoryRepository repository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<ReserveInventoryCommand, ReservationResponse>
    {
        public async Task<ReservationResponse> Handle(
            ReserveInventoryCommand command,
            CancellationToken cancellationToken
        )
        {
            var entity = await Find(
                repository,
                command.ProductId,
                command.WarehouseId,
                cancellationToken
            );
            var reservation = entity.Reserve(command.OrderId, command.Quantity);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Map(reservation);
        }
    }
}
