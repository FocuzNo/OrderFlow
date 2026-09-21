using OrderFlow.Inventory.Domain.Reservations;
using OrderFlow.Inventory.Domain.Stock;
using OrderFlow.Inventory.Domain.Warehouses;

namespace OrderFlow.Inventory.Application.Abstractions.Persistence;

public interface IInventoryRepository : IRepository<StockItem>, IRepository<Warehouse>
{
    Task<IReadOnlyList<Warehouse>> ListWarehousesAsync(CancellationToken cancellationToken);
    Task<StockItem?> GetStockAsync(
        Guid productId,
        Guid warehouseId,
        CancellationToken cancellationToken
    );
    Task<StockItem?> GetBestAvailableStockAsync(
        Guid productId,
        CancellationToken cancellationToken
    );
    Task<IReadOnlyList<StockItem>> GetStockItemsWithPendingReservationsAsync(
        Guid orderId,
        CancellationToken cancellationToken
    );
    Task<StockReservation?> GetReservationAsync(Guid id, CancellationToken cancellationToken);
}
