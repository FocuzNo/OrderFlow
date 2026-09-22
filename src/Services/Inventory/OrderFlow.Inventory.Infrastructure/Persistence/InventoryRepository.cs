using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Reservations;
using OrderFlow.Inventory.Domain.Stock;
using OrderFlow.Inventory.Domain.Warehouses;
using OrderFlow.Inventory.Infrastructure.Persistence.Repositories;

namespace OrderFlow.Inventory.Infrastructure.Persistence;

public sealed class InventoryRepository(InventoryDbContext databaseContext)
    : Repository<StockItem>(databaseContext),
        IInventoryRepository
{
    public void Remove(Warehouse warehouse) => DatabaseContext.Warehouses.Remove(warehouse);

    public Task<StockItem?> GetByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken
    ) =>
        DatabaseContext
            .StockItems.Include(stock => stock.Reservations)
            .SingleOrDefaultAsync(stock => stock.ProductId == productId, cancellationToken);

    public async Task<IReadOnlyList<StockItem>> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken
    ) =>
        await DatabaseContext
            .StockItems.AsNoTracking()
            .Include(stock => stock.Reservations)
            .OrderBy(entity => entity.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task AddAsync(Warehouse candidate, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        DatabaseContext.Warehouses.Add(candidate);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<Warehouse>> ListWarehousesAsync(
        CancellationToken cancellationToken
    ) =>
        await DatabaseContext
            .Warehouses.AsNoTracking()
            .OrderBy(candidate => candidate.Name)
            .ToListAsync(cancellationToken);

    public Task<StockItem?> GetStockAsync(
        Guid productId,
        Guid warehouseId,
        CancellationToken cancellationToken
    ) =>
        DatabaseContext
            .StockItems.Include(candidate => candidate.Reservations)
            .SingleOrDefaultAsync(
                candidate =>
                    candidate.ProductId == productId && candidate.WarehouseId == warehouseId,
                cancellationToken
            );

    public Task<StockReservation?> GetReservationAsync(
        Guid id,
        CancellationToken cancellationToken
    ) =>
        DatabaseContext
            .Reservations.AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
}
