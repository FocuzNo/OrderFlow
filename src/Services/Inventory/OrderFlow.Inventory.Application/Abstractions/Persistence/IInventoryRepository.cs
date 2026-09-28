using OrderFlow.Inventory.Domain.Reservations;
using OrderFlow.Inventory.Domain.Stock;
using OrderFlow.Inventory.Domain.Warehouses;

namespace OrderFlow.Inventory.Application.Abstractions.Persistence;

public interface IInventoryRepository
{
    Task<StockItem?> GetByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken
    );

    Task<IReadOnlyCollection<StockItem>> GetByProductIdsAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken
    );

    Task<StockItem?> GetStockAsync(
        Guid productId,
        Guid warehouseId,
        CancellationToken cancellationToken
    );

    Task<IReadOnlyList<StockItem>> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken
    );

    Task<IReadOnlyList<Warehouse>> ListWarehousesAsync(
        CancellationToken cancellationToken
    );

    Task<StockReservation?> GetReservationAsync(
        Guid id,
        CancellationToken cancellationToken
    );

    Task AddAsync(
        StockItem entity,
        CancellationToken cancellationToken
    );

    Task AddAsync(
        Warehouse entity,
        CancellationToken cancellationToken
    );

    void Remove(
        StockItem entity
    );

    void Remove(
        Warehouse entity
    );
}
