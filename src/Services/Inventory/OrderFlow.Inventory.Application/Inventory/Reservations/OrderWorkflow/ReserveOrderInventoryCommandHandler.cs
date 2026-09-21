using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Stock;

namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class ReserveOrderInventoryCommandHandler(
        IInventoryRepository repository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<ReserveOrderInventoryCommand, InventoryReservationResult>
    {
        public async Task<InventoryReservationResult> Handle(
            ReserveOrderInventoryCommand command,
            CancellationToken cancellationToken
        )
        {
            var requestedItems = command
                .Items.GroupBy(item => item.ProductId)
                .Select(group => new OrderInventoryItem(
                    group.Key,
                    group.Sum(item => item.Quantity)
                ))
                .ToArray();

            var selectedStock = new List<(StockItem Stock, int Quantity)>();
            foreach (var item in requestedItems)
            {
                var stock = await repository.GetBestAvailableStockAsync(
                    item.ProductId,
                    cancellationToken
                );
                if (stock is null || stock.AvailableQuantity < item.Quantity)
                {
                    return new InventoryReservationResult(
                        false,
                        [],
                        $"Insufficient stock for product {item.ProductId}."
                    );
                }

                selectedStock.Add((stock, item.Quantity));
            }

            var reservationIds = selectedStock
                .Select(item => item.Stock.Reserve(command.OrderId, item.Quantity).Id)
                .ToArray();

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return new InventoryReservationResult(true, reservationIds, null);
        }
    }
}
