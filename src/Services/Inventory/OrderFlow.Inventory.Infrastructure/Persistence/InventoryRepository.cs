using Microsoft.EntityFrameworkCore;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Reservations;
using OrderFlow.Inventory.Domain.Stock;
using OrderFlow.Inventory.Domain.Warehouses;

namespace OrderFlow.Inventory.Infrastructure.Persistence;

public sealed class InventoryRepository(InventoryDbContext db) : IInventoryRepository
{
    public async Task AddWarehouseAsync(Warehouse x, CancellationToken ct)
    {
        db.Warehouses.Add(x);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Warehouse>> ListWarehousesAsync(CancellationToken ct) =>
        await db.Warehouses.AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);

    public async Task AddStockItemAsync(StockItem x, CancellationToken ct)
    {
        db.StockItems.Add(x);
        await db.SaveChangesAsync(ct);
    }

    public Task<StockItem?> GetStockAsync(Guid p, Guid w, CancellationToken ct) =>
        db
            .StockItems.Include(x => x.Reservations)
            .SingleOrDefaultAsync(x => x.ProductId == p && x.WarehouseId == w, ct);

    public Task<StockItem?> GetBestAvailableStockAsync(Guid productId, CancellationToken ct) =>
        db
            .StockItems.Include(x => x.Reservations)
            .Where(x => x.ProductId == productId)
            .OrderByDescending(x => x.QuantityOnHand - x.ReservedQuantity)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<StockItem>> GetStockItemsWithPendingReservationsAsync(
        Guid orderId,
        CancellationToken ct
    ) =>
        await db
            .StockItems.Include(x => x.Reservations)
            .Where(x =>
                x.Reservations.Any(reservation =>
                    reservation.OrderId == orderId
                    && reservation.Status == ReservationStatus.Pending
                )
            )
            .ToListAsync(ct);

    public Task<StockReservation?> GetReservationAsync(Guid id, CancellationToken ct) =>
        db.Reservations.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);

    public async Task SaveAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
}
