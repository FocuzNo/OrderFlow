using OrderFlow.Ordering.Application.Abstractions.Messaging;
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
        var publisher = new OrderCreatedPublisher();

        var command = new OrderFeatures.CreateOrderCommand(
            Guid.NewGuid(),
            "buyer@example.test",
            new("Main", "Minsk", "220000", "BY"),
            [new(Guid.NewGuid(), "Notebook", 5, 3)]
        );

        var handler = new OrderFeatures.CreateOrderCommandHandler(
            repository,
            commit,
            publisher
        );

        var response = await handler.Handle(
            command,
            CancellationToken.None
        );

        Assert.Equal(15, repository.Entity!.TotalAmount);
        Assert.Equal(1, commit.Count);
        Assert.Equal(1, publisher.Count);
        Assert.Equal(repository.Entity.Id, response.Id);
        Assert.Equal(repository.Entity.Id, publisher.Order!.Id);
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

        Assert.False(
            new CreateOrderCommandValidator()
                .Validate(command)
                .IsValid
        );
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

        Assert.False(
            new CreateOrderCommandValidator()
                .Validate(command)
                .IsValid
        );
    }

    private sealed class Commit : IUnitOfWork
    {
        public int Count { get; private set; }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default
        )
        {
            Count++;

            return Task.FromResult(1);
        }
    }

    private sealed class Repository : IOrderRepository
    {
        public Order? Entity { get; private set; }

        public Task AddAsync(
            Order entity,
            CancellationToken cancellationToken
        )
        {
            Entity = entity;

            return Task.CompletedTask;
        }

        public Task<Order?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken
        ) =>
            Task.FromResult(Entity);

        public Task<IReadOnlyList<Order>> ListAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken
        ) =>
            Task.FromResult<IReadOnlyList<Order>>([]);

        public Task<IReadOnlyList<Order>> GetCustomerOrdersAsync(
            Guid customerId,
            CancellationToken cancellationToken
        ) =>
            Task.FromResult<IReadOnlyList<Order>>([]);
    }

    private sealed class OrderCreatedPublisher : IOrderCreatedPublisher
    {
        public int Count { get; private set; }

        public Order? Order { get; private set; }

        public Task PublishAsync(
            Order order,
            CancellationToken cancellationToken
        )
        {
            Count++;
            Order = order;

            return Task.CompletedTask;
        }
    }
}
