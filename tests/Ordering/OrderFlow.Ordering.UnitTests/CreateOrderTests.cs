using OrderFlow.Ordering.Application.Abstractions.Persistence;
using OrderFlow.Ordering.Application.Orders;
using OrderFlow.Ordering.Domain.Orders;

namespace OrderFlow.Ordering.UnitTests;

public sealed class CreateOrderTests
{
    [Fact]
    public async Task Create_ShouldCommitSnapshotsAndCalculatedTotal()
    {
        var repository = new Repository();
        var commit = new Commit();
        var command = new OrderFeatures.CreateOrderCommand(
            Guid.NewGuid(),
            "buyer@example.test",
            new("Main", "Minsk", "220000", "BY"),
            [new(Guid.NewGuid(), "Notebook", 5, 3)]
        );
        var response = await new OrderFeatures.CreateOrderCommandHandler(repository, commit).Handle(
            command,
            default
        );
        Assert.Equal(15, repository.Entity!.TotalAmount);
        Assert.Equal(1, commit.Count);
        Assert.Equal(repository.Entity.Id, response.Id);
    }

    [Fact]
    public void Validator_ShouldRejectEmptyItemsAndMissingAddress()
    {
        var command = new OrderFeatures.CreateOrderCommand(
            Guid.NewGuid(),
            "buyer@example.test",
            null!,
            []
        );
        Assert.False(new CreateOrderCommandValidator().Validate(command).IsValid);
    }

    [Fact]
    public void Validator_ShouldRejectNullItemWithoutThrowing()
    {
        var command = new OrderFeatures.CreateOrderCommand(
            Guid.NewGuid(),
            "buyer@example.test",
            new("Main", "Minsk", "220000", "BY"),
            [null!]
        );

        Assert.False(new CreateOrderCommandValidator().Validate(command).IsValid);
    }

    private sealed class Commit : IUnitOfWork
    {
        public int Count { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            Count++;
            return Task.FromResult(1);
        }
    }

    private sealed class Repository : IOrderRepository
    {
        public Order? Entity { get; private set; }

        public Task AddAsync(Order entity, CancellationToken cancellationToken)
        {
            Entity = entity;
            return Task.CompletedTask;
        }

        public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Entity);

        public Task<IReadOnlyList<Order>> ListAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken
        ) => Task.FromResult<IReadOnlyList<Order>>([]);

        public Task<IReadOnlyList<Order>> GetCustomerOrdersAsync(
            Guid customerId,
            CancellationToken cancellationToken
        ) => Task.FromResult<IReadOnlyList<Order>>([]);
    }
}
