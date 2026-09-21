using OrderFlow.Payments.Application;
using OrderFlow.Payments.Domain;

namespace OrderFlow.Payments.UnitTests;

public sealed class PaymentTests
{
    [Fact] public void Create_rejects_negative_amount() => Assert.Throws<ArgumentOutOfRangeException>(() => Payment.Create("order", null, -0.01m));
    [Fact] public async Task Handler_creates_and_persists_payment()
    {
        var repository = new Repository();
        var result = await new CreateHandler(repository).Handle(new CreatePaymentCommand("order-1", "provider", 99), default);
        Assert.Equal(result.Id, repository.Entity?.Id); Assert.Equal(99, result.Value); Assert.NotEmpty(repository.Entity!.DomainEvents);
    }
    private sealed class Repository : IPaymentRepository
    {
        public Payment? Entity { get; private set; }
        public Task AddAsync(Payment entity, CancellationToken ct) { Entity = entity; return Task.CompletedTask; }
        public Task<Payment?> GetAsync(Guid id, CancellationToken ct) => Task.FromResult(Entity);
        public Task<IReadOnlyList<Payment>> ListAsync(CancellationToken ct) => Task.FromResult<IReadOnlyList<Payment>>(Entity is null ? [] : [Entity]);
        public Task SaveAsync(CancellationToken ct) => Task.CompletedTask;
        public Task DeleteAsync(Payment entity, CancellationToken ct) => Task.CompletedTask;
    }
}
