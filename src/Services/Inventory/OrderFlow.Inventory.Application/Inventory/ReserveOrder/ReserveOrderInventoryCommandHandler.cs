using OrderFlow.Inventory.Application.Abstractions.Messaging;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Stock;

namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    private const string OrderCreatedEventType =
        "OrderCreatedIntegrationEvent";

    public sealed class ReserveOrderInventoryCommandHandler(
        IInventoryRepository repository,
        IInboxRepository inboxRepository,
        IInventoryOutboxWriter outboxWriter,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<
        ReserveOrderInventoryCommand,
        ReserveOrderInventoryResponse
    >
    {
        public async Task<ReserveOrderInventoryResponse> Handle(
            ReserveOrderInventoryCommand command,
            CancellationToken cancellationToken
        )
        {
            var alreadyProcessed =
                await inboxRepository.ExistsAsync(
                    command.EventId,
                    cancellationToken
                );

            if (alreadyProcessed)
            {
                return new ReserveOrderInventoryResponse(
                    true,
                    false,
                    null,
                    []
                );
            }

            var requestedItems =
                command.Items
                    .GroupBy(item =>
                        item.ProductId
                    )
                    .Select(group =>
                        new ReserveOrderInventoryItem(
                            group.Key,
                            group.Sum(item =>
                                item.Quantity
                            )
                        )
                    )
                    .ToArray();

            var productIds =
                requestedItems
                    .Select(item =>
                        item.ProductId
                    )
                    .ToArray();

            var stockItems =
                await repository.GetByProductIdsAsync(
                    productIds,
                    cancellationToken
                );

            var selectedStockItems =
                new List<(
                    StockItem StockItem,
                    ReserveOrderInventoryItem RequestedItem
                )>();

            foreach (var requestedItem in requestedItems)
            {
                var stockItem =
                    stockItems
                        .Where(candidate =>
                            candidate.ProductId
                            == requestedItem.ProductId
                        )
                        .Where(candidate =>
                            candidate.AvailableQuantity
                            >= requestedItem.Quantity
                        )
                        .OrderByDescending(candidate =>
                            candidate.AvailableQuantity
                        )
                        .ThenBy(candidate =>
                            candidate.WarehouseId
                        )
                        .FirstOrDefault();

                if (stockItem is null)
                {
                    var reason =
                        $"Insufficient stock for product {requestedItem.ProductId}.";

                    return await FailAsync(
                        command,
                        reason,
                        cancellationToken
                    );
                }

                selectedStockItems.Add(
                    (
                        stockItem,
                        requestedItem
                    )
                );
            }

            var reservedItems =
                new List<ReservedStockItem>();

            foreach (var selection in selectedStockItems)
            {
                var reservation =
                    selection.StockItem.Reserve(
                        command.OrderId,
                        selection.RequestedItem.Quantity
                    );

                reservedItems.Add(
                    new ReservedStockItem(
                        selection.RequestedItem.ProductId,
                        selection.StockItem.Id,
                        selection.StockItem.WarehouseId,
                        reservation.Id,
                        selection.RequestedItem.Quantity
                    )
                );
            }

            inboxRepository.Add(
                command.EventId,
                OrderCreatedEventType,
                DateTimeOffset.UtcNow
            );

            outboxWriter.AddReserved(
                command.OrderId,
                reservedItems
            );

            await unitOfWork.SaveChangesAsync(
                cancellationToken
            );

            return new ReserveOrderInventoryResponse(
                false,
                true,
                null,
                reservedItems
            );
        }

        private async Task<ReserveOrderInventoryResponse> FailAsync(
            ReserveOrderInventoryCommand command,
            string reason,
            CancellationToken cancellationToken
        )
        {
            inboxRepository.Add(
                command.EventId,
                OrderCreatedEventType,
                DateTimeOffset.UtcNow
            );

            outboxWriter.AddReservationFailed(
                command.OrderId,
                reason
            );

            await unitOfWork.SaveChangesAsync(
                cancellationToken
            );

            return new ReserveOrderInventoryResponse(
                false,
                false,
                reason,
                []
            );
        }
    }
}
