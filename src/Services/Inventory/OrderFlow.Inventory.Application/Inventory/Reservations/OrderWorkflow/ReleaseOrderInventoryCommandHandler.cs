using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Reservations;

namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class ReleaseOrderInventoryCommandHandler(IInventoryRepository repository)
        : IRequestHandler<ReleaseOrderInventoryCommand>
    {
        public async Task Handle(
            ReleaseOrderInventoryCommand command,
            CancellationToken cancellationToken
        )
        {
            var stockItems = await repository.GetStockItemsWithPendingReservationsAsync(
                command.OrderId,
                cancellationToken
            );

            foreach (var stockItem in stockItems)
            {
                var reservationIds = stockItem
                    .Reservations.Where(reservation =>
                        reservation.OrderId == command.OrderId
                        && reservation.Status == ReservationStatus.Pending
                    )
                    .Select(reservation => reservation.Id)
                    .ToArray();

                foreach (var reservationId in reservationIds)
                    stockItem.Release(reservationId);
            }

            await repository.SaveAsync(cancellationToken);
        }
    }
}
