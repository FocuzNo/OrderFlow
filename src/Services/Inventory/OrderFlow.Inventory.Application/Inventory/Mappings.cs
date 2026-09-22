using OrderFlow.Inventory.Application.Abstractions.Errors;
using OrderFlow.Inventory.Application.Abstractions.Messaging;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Stock;
using OrderFlow.Inventory.Domain.Warehouses;

namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    private static async Task<StockItem> Find(
        IInventoryRepository repository,
        Guid productId,
        Guid warehouseId,
        CancellationToken cancellationToken
    ) =>
        await repository.GetStockAsync(productId, warehouseId, cancellationToken)
        ?? throw new NotFoundException("Stock item was not found.");

    private static StockResponse Map(StockItem entity) =>
        new(
            entity.Id,
            entity.ProductId,
            entity.WarehouseId,
            entity.Sku,
            entity.QuantityOnHand,
            entity.ReservedQuantity,
            entity.AvailableQuantity
        );

    private static ReservationResponse Map(Domain.Reservations.StockReservation entity) =>
        new(entity.Id, entity.OrderId, entity.StockItemId, entity.Quantity, entity.Status.Name);
}
