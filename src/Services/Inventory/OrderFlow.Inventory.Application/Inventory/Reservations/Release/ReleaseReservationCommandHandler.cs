using OrderFlow.Inventory.Application.Abstractions.Errors;
using OrderFlow.Inventory.Application.Abstractions.Messaging;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Stock;
using OrderFlow.Inventory.Domain.Warehouses;

namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class ReleaseReservationCommandHandler(
        IInventoryRepository repository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<ReleaseReservationCommand>
    {
        public async Task Handle(
            ReleaseReservationCommand command,
            CancellationToken cancellationToken
        )
        {
            var entity = await Find(
                repository,
                command.ProductId,
                command.WarehouseId,
                cancellationToken
            );
            entity.Release(command.ReservationId);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
