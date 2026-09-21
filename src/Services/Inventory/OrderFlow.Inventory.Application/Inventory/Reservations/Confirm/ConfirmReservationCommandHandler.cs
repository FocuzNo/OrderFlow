using OrderFlow.Inventory.Application.Abstractions.Errors;
using OrderFlow.Inventory.Application.Abstractions.Messaging;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Stock;
using OrderFlow.Inventory.Domain.Warehouses;

namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class ConfirmReservationCommandHandler(
        IInventoryRepository repository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<ConfirmReservationCommand>
    {
        public async Task Handle(
            ConfirmReservationCommand command,
            CancellationToken cancellationToken
        )
        {
            var entity = await Find(
                repository,
                command.ProductId,
                command.WarehouseId,
                cancellationToken
            );
            entity.Confirm(command.ReservationId);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
