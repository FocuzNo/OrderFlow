using OrderFlow.Inventory.Application.Abstractions.Errors;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Application.Inventory;
using OrderFlow.Inventory.Domain.Reservations;
using OrderFlow.Inventory.Domain.Stock;
using OrderFlow.Inventory.Domain.Warehouses;

namespace OrderFlow.Inventory.UnitTests;

public sealed class InventoryCommandTests
{
    [Fact]
    public async Task Update_ShouldCommitQuantityChangeOnce()
    {
        var stock = StockItem.Create(Guid.NewGuid(), Guid.NewGuid(), "SKU");
        var repository = new TestInventoryRepository { Stock = stock };
        var unitOfWork = new TestUnitOfWork();
        var handler = new InventoryFeatures.UpdateInventoryCommandHandler(repository, unitOfWork);

        await handler.Handle(new(stock.ProductId, 12), default);

        Assert.Equal(12, stock.QuantityOnHand);
        Assert.Equal(1, unitOfWork.CommitCount);
    }

    [Fact]
    public async Task Delete_ShouldRejectMissingProductWithoutCommit()
    {
        var unitOfWork = new TestUnitOfWork();
        var handler = new InventoryFeatures.DeleteInventoryCommandHandler(
            new TestInventoryRepository(),
            unitOfWork
        );

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new(Guid.NewGuid()), default)
        );

        Assert.Equal(0, unitOfWork.CommitCount);
    }

    private sealed class TestUnitOfWork : IUnitOfWork
    {
        public int CommitCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            CommitCount++;
            return Task.FromResult(1);
        }
    }

    private sealed class TestInventoryRepository : IInventoryRepository
    {
        public StockItem? Stock { get; init; }

        public Task<StockItem?> GetByProductIdAsync(
            Guid productId,
            CancellationToken cancellationToken
        ) => Task.FromResult(Stock?.ProductId == productId ? Stock : null);

        public Task<StockItem?> GetStockAsync(
            Guid productId,
            Guid warehouseId,
            CancellationToken cancellationToken
        ) =>
            Task.FromResult(
                Stock?.ProductId == productId && Stock.WarehouseId == warehouseId ? Stock : null
            );

        public Task<IReadOnlyList<StockItem>> ListAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken
        ) => throw new NotSupportedException();

        public Task<IReadOnlyList<Warehouse>> ListWarehousesAsync(
            CancellationToken cancellationToken
        ) => throw new NotSupportedException();

        public Task<StockReservation?> GetReservationAsync(
            Guid id,
            CancellationToken cancellationToken
        ) => throw new NotSupportedException();

        public Task AddAsync(StockItem entity, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task AddAsync(Warehouse entity, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public void Remove(StockItem entity) => throw new NotSupportedException();

        public void Remove(Warehouse entity) => throw new NotSupportedException();
    }
}
