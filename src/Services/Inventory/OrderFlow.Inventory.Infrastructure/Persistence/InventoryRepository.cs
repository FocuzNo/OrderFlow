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

    public Task<StockItem?> GetBestAvailableStockAsync(
        Guid productId,
        CancellationToken cancellationToken
    ) =>
        DatabaseContext
            .StockItems.Include(candidate => candidate.Reservations)
            .Where(candidate => candidate.ProductId == productId)
            .OrderByDescending(candidate => candidate.QuantityOnHand - candidate.ReservedQuantity)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<StockItem>> GetStockItemsWithPendingReservationsAsync(
        Guid orderId,
        CancellationToken cancellationToken
    ) =>
        await DatabaseContext
            .StockItems.Include(candidate => candidate.Reservations)
            .Where(candidate =>
                candidate.Reservations.Any(reservation =>
                    reservation.OrderId == orderId
                    && reservation.Status == ReservationStatus.Pending
                )
            )
            .ToListAsync(cancellationToken);

    public Task<StockReservation?> GetReservationAsync(
        Guid id,
        CancellationToken cancellationToken
    ) =>
        DatabaseContext
            .Reservations.AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
}
