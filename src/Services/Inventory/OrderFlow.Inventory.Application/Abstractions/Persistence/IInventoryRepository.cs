using OrderFlow.Inventory.Domain.Reservations;
using OrderFlow.Inventory.Domain.Stock;
using OrderFlow.Inventory.Domain.Warehouses;

namespace OrderFlow.Inventory.Application.Abstractions.Persistence;

public interface IInventoryRepository : IRepository<StockItem>, IRepository<Warehouse>
{
    Task<StockItem?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken);
    Task<IReadOnlyList<StockItem>> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken
    );
    Task<IReadOnlyList<Warehouse>> ListWarehousesAsync(CancellationToken cancellationToken);
    Task<StockItem?> GetStockAsync(
        Guid productId,
        Guid warehouseId,
        CancellationToken cancellationToken
    );
    Task<StockReservation?> GetReservationAsync(Guid id, CancellationToken cancellationToken);
}
