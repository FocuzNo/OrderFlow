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
        var stock =
            StockItem.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "SKU"
            );

        var repository =
            new TestInventoryRepository
            {
                Stock = stock,
            };

        var unitOfWork =
            new TestUnitOfWork();

        var handler =
            new InventoryFeatures.UpdateInventoryCommandHandler(
                repository,
                unitOfWork
            );

        await handler.Handle(
            new(
                stock.ProductId,
                12
            ),
            default
        );

        Assert.Equal(
            12,
            stock.QuantityOnHand
        );

        Assert.Equal(
            1,
            unitOfWork.CommitCount
        );
    }

    [Fact]
    public async Task Delete_ShouldRejectMissingProductWithoutCommit()
    {
        var unitOfWork =
            new TestUnitOfWork();

        var handler =
            new InventoryFeatures.DeleteInventoryCommandHandler(
                new TestInventoryRepository(),
                unitOfWork
            );

        await Assert.ThrowsAsync<NotFoundException>(
            () =>
                handler.Handle(
                    new(
                        Guid.NewGuid()
                    ),
                    default
                )
        );

        Assert.Equal(
            0,
            unitOfWork.CommitCount
        );
    }

    [Fact]
    public async Task ReserveOrder_ShouldReserveGroupedItemsAndCommitInboxAndOutboxOnce()
    {
        var stockItem = StockItem.Create(Guid.NewGuid(), Guid.NewGuid(), "SKU");
        stockItem.UpdateQuantity(10);
        var repository = new TestInventoryRepository { Stock = stockItem };
        var inbox = new TestInboxRepository();
        var outbox = new TestOutboxWriter();
        var unitOfWork = new TestUnitOfWork();
        var handler = new InventoryFeatures.ReserveOrderInventoryCommandHandler(
            repository, inbox, outbox, unitOfWork
        );
        var command = new ReserveOrderInventoryCommand(
            Guid.NewGuid(), Guid.NewGuid(),
            [new(stockItem.ProductId, 2), new(stockItem.ProductId, 3)]
        );

        var response = await handler.Handle(command, default);
        var duplicate = await handler.Handle(command, default);

        Assert.True(response.IsReserved);
        Assert.True(duplicate.IsDuplicate);
        Assert.Equal(5, stockItem.ReservedQuantity);
        Assert.Single(stockItem.Reservations);
        Assert.Equal(1, unitOfWork.CommitCount);
        Assert.Equal(1, outbox.ReservedCount);
        Assert.Equal(0, outbox.FailedCount);
        Assert.Contains(command.EventId, inbox.EventIds);
    }

    [Fact]
    public async Task ReserveOrder_ShouldNotPartiallyReserve_WhenLaterProductIsMissing()
    {
        var stockItem = StockItem.Create(Guid.NewGuid(), Guid.NewGuid(), "SKU");
        stockItem.UpdateQuantity(10);
        var inbox = new TestInboxRepository();
        var outbox = new TestOutboxWriter();
        var unitOfWork = new TestUnitOfWork();
        var handler = new InventoryFeatures.ReserveOrderInventoryCommandHandler(
            new TestInventoryRepository { Stock = stockItem }, inbox, outbox, unitOfWork
        );
        var response = await handler.Handle(
            new ReserveOrderInventoryCommand(
                Guid.NewGuid(), Guid.NewGuid(),
                [new(stockItem.ProductId, 2), new(Guid.NewGuid(), 1)]
            ),
            default
        );

        Assert.False(response.IsReserved);
        Assert.Empty(stockItem.Reservations);
        Assert.Equal(0, stockItem.ReservedQuantity);
        Assert.Single(inbox.EventIds);
        Assert.Equal(1, outbox.FailedCount);
        Assert.Equal(0, outbox.ReservedCount);
        Assert.Equal(1, unitOfWork.CommitCount);
    }

    [Fact]
    public void ReserveOrderValidator_ShouldRejectInvalidContract()
    {
        var validator = new ReserveOrderInventoryCommandValidator();
        Assert.False(validator.Validate(new ReserveOrderInventoryCommand(Guid.Empty, Guid.Empty, [])).IsValid);
        Assert.False(validator.Validate(new ReserveOrderInventoryCommand(Guid.NewGuid(), Guid.NewGuid(), [null!])).IsValid);
        Assert.False(validator.Validate(new ReserveOrderInventoryCommand(Guid.NewGuid(), Guid.NewGuid(), [new(Guid.NewGuid(), 0)])).IsValid);
    }

    private sealed class TestInboxRepository : IInboxRepository
    {
        public HashSet<Guid> EventIds { get; } = [];

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(EventIds.Contains(id));

        public void Add(Guid id, string type, DateTimeOffset processedAt) => EventIds.Add(id);
    }

    private sealed class TestOutboxWriter
        : OrderFlow.Inventory.Application.Abstractions.Messaging.IInventoryOutboxWriter
    {
        public int ReservedCount { get; private set; }

        public int FailedCount { get; private set; }

        public void AddReserved(Guid orderId, IReadOnlyCollection<ReservedStockItem> items) =>
            ReservedCount++;

        public void AddReservationFailed(Guid orderId, string reason) => FailedCount++;
    }

    private sealed class TestUnitOfWork
        : IUnitOfWork
    {
        public int CommitCount { get; private set; }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default
        )
        {
            CommitCount++;

            return Task.FromResult(
                1
            );
        }
    }

    private sealed class TestInventoryRepository
        : IInventoryRepository
    {
        public StockItem? Stock { get; init; }

        public Task<StockItem?> GetByProductIdAsync(
            Guid productId,
            CancellationToken cancellationToken
        ) =>
            Task.FromResult(
                Stock?.ProductId == productId
                    ? Stock
                    : null
            );

        public Task<IReadOnlyCollection<StockItem>> GetByProductIdsAsync(
            IReadOnlyCollection<Guid> productIds,
            CancellationToken cancellationToken
        )
        {
            IReadOnlyCollection<StockItem> result =
                Stock is not null
                && productIds.Contains(
                    Stock.ProductId
                )
                    ? [Stock]
                    : [];

            return Task.FromResult(
                result
            );
        }

        public Task<StockItem?> GetStockAsync(
            Guid productId,
            Guid warehouseId,
            CancellationToken cancellationToken
        ) =>
            Task.FromResult(
                Stock?.ProductId == productId
                && Stock.WarehouseId == warehouseId
                    ? Stock
                    : null
            );

        public Task<IReadOnlyList<StockItem>> ListAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken
        ) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<Warehouse>> ListWarehousesAsync(
            CancellationToken cancellationToken
        ) =>
            throw new NotSupportedException();

        public Task<StockReservation?> GetReservationAsync(
            Guid id,
            CancellationToken cancellationToken
        ) =>
            throw new NotSupportedException();

        public Task AddAsync(
            StockItem entity,
            CancellationToken cancellationToken
        ) =>
            throw new NotSupportedException();

        public Task AddAsync(
            Warehouse entity,
            CancellationToken cancellationToken
        ) =>
            throw new NotSupportedException();

        public void Remove(
            StockItem entity
        ) =>
            throw new NotSupportedException();

        public void Remove(
            Warehouse entity
        ) =>
            throw new NotSupportedException();
    }
}
