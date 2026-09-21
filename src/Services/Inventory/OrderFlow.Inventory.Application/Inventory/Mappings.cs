using MediatR;
using OrderFlow.Inventory.Application.Abstractions.Errors;
using OrderFlow.Inventory.Application.Abstractions.Messaging;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Stock;
using OrderFlow.Inventory.Domain.Warehouses;

namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    private static async Task<StockItem> Find(
        IInventoryRepository r,
        Guid p,
        Guid w,
        CancellationToken ct
    ) =>
        await r.GetStockAsync(p, w, ct) ?? throw new NotFoundException("Stock item was not found.");

    private static StockResponse Map(StockItem x) =>
        new(
            x.Id,
            x.ProductId,
            x.WarehouseId,
            x.Sku,
            x.QuantityOnHand,
            x.ReservedQuantity,
            x.AvailableQuantity
        );

    private static ReservationResponse Map(Domain.Reservations.StockReservation x) =>
        new(x.Id, x.OrderId, x.StockItemId, x.Quantity, x.Status.Name);
}
