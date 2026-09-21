using OrderFlow.Ordering.Application;
using OrderFlow.Ordering.Domain;

namespace OrderFlow.Ordering.UnitTests;

public sealed class OrderTests
{
    [Fact] public void Create_enforces_value_invariant() => Assert.Throws<ArgumentOutOfRangeException>(() => Order.Create("customer", null, -1));
    [Fact] public async Task Handler_creates_and_persists_order()
    {
        var repository = new Repository();
        var result = await new CreateHandler(repository).Handle(new CreateOrderCommand("customer", "notes", 42), default);
        Assert.Equal(result.Id, repository.Entity?.Id); Assert.Equal("Pending", result.Status); Assert.Contains(repository.Entity!.DomainEvents, x => x.Type == "ordering.order-created");
    }
    private sealed class Repository : IOrderRepository
    {
        public Order? Entity { get; private set; }
        public Task AddAsync(Order entity, CancellationToken ct) { Entity = entity; return Task.CompletedTask; }
        public Task<Order?> GetAsync(Guid id, CancellationToken ct) => Task.FromResult(Entity);
        public Task<IReadOnlyList<Order>> ListAsync(CancellationToken ct) => Task.FromResult<IReadOnlyList<Order>>(Entity is null ? [] : [Entity]);
        public Task SaveAsync(CancellationToken ct) => Task.CompletedTask;
        public Task DeleteAsync(Order entity, CancellationToken ct) => Task.CompletedTask;
    }
}
