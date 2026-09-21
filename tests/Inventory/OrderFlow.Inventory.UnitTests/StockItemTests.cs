using OrderFlow.Inventory.Application;
using OrderFlow.Inventory.Domain;

namespace OrderFlow.Inventory.UnitTests;

public sealed class StockItemTests
{
    [Fact] public void Create_rejects_blank_reference() => Assert.Throws<ArgumentException>(() => StockItem.Create(" ", null, 1));
    [Fact] public async Task Handler_creates_and_persists_stock_item()
    {
        var repository = new Repository();
        var result = await new CreateHandler(repository).Handle(new CreateStockItemCommand("SKU-1", "product", 10), default);
        Assert.Equal(result.Id, repository.Entity?.Id); Assert.Equal(10, result.Value); Assert.NotEmpty(repository.Entity!.DomainEvents);
    }
    private sealed class Repository : IStockItemRepository
    {
        public StockItem? Entity { get; private set; }
        public Task AddAsync(StockItem entity, CancellationToken ct) { Entity = entity; return Task.CompletedTask; }
        public Task<StockItem?> GetAsync(Guid id, CancellationToken ct) => Task.FromResult(Entity);
        public Task<IReadOnlyList<StockItem>> ListAsync(CancellationToken ct) => Task.FromResult<IReadOnlyList<StockItem>>(Entity is null ? [] : [Entity]);
        public Task SaveAsync(CancellationToken ct) => Task.CompletedTask;
        public Task DeleteAsync(StockItem entity, CancellationToken ct) => Task.CompletedTask;
    }
}
