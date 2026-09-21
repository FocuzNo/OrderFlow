using OrderFlow.Inventory.Domain.Reservations;
using OrderFlow.Inventory.Domain.Stock;
using OrderFlow.Inventory.Domain.Warehouses;
namespace OrderFlow.Inventory.Application.Abstractions.Persistence;
public interface IInventoryRepository
{
    Task AddWarehouseAsync(Warehouse warehouse, CancellationToken ct); Task<IReadOnlyList<Warehouse>> ListWarehousesAsync(CancellationToken ct);
    Task AddStockItemAsync(StockItem item, CancellationToken ct); Task<StockItem?> GetStockAsync(Guid productId, Guid warehouseId, CancellationToken ct); Task<StockReservation?> GetReservationAsync(Guid id, CancellationToken ct); Task SaveAsync(CancellationToken ct);
}
