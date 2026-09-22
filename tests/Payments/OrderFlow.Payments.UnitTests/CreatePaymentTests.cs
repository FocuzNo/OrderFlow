using OrderFlow.Payments.Application.Abstractions.Errors;
using OrderFlow.Payments.Application.Abstractions.Persistence;
using OrderFlow.Payments.Application.Payments;
using OrderFlow.Payments.Domain.Payments;

namespace OrderFlow.Payments.UnitTests;

public sealed class CreatePaymentTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Create_ShouldCommitSimulatedResult(bool failure)
    {
        var repository = new Repository();
        var commit = new Commit();
        var response = await new PaymentFeatures.CreatePaymentCommandHandler(
            repository,
            commit
        ).Handle(new(Guid.NewGuid(), 10, "Card", failure), default);
        Assert.Equal(failure ? "Failed" : "Succeeded", response.Status);
        Assert.Equal(1, commit.Count);
        Assert.NotNull(repository.Entity);
    }

    [Fact]
    public async Task Create_ShouldRejectDuplicateOrderWithoutCommit()
    {
        var repository = new Repository
        {
            Entity = Payment.Create(Guid.NewGuid(), 10, PaymentMethod.Card),
        };
        var commit = new Commit();
        await Assert.ThrowsAsync<ConflictException>(() =>
            new PaymentFeatures.CreatePaymentCommandHandler(repository, commit).Handle(
                new(repository.Entity.OrderId, 10, "Card"),
                default
            )
        );
        Assert.Equal(0, commit.Count);
    }

    [Theory]
    [InlineData(0, "Card")]
    [InlineData(10, "Unknown")]
    public void Validator_ShouldRejectInvalidAmountOrMethod(decimal amount, string method) =>
        Assert.False(
            new CreatePaymentCommandValidator()
                .Validate(new PaymentFeatures.CreatePaymentCommand(Guid.NewGuid(), amount, method))
                .IsValid
        );

    private sealed class Commit : IUnitOfWork
    {
        public int Count { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            Count++;
            return Task.FromResult(1);
        }
    }

    private sealed class Repository : IPaymentRepository
    {
        public Payment? Entity { get; set; }

        public Task AddAsync(Payment entity, CancellationToken cancellationToken)
        {
            Entity = entity;
            return Task.CompletedTask;
        }

        public Task<Payment?> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken) =>
            Task.FromResult(Entity?.OrderId == orderId ? Entity : null);

        public Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Entity?.Id == id ? Entity : null);

        public Task<IReadOnlyList<Payment>> ListAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken
        ) => Task.FromResult<IReadOnlyList<Payment>>(Entity is null ? [] : [Entity]);
    }
}
