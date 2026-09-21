using OrderFlow.Notifications.Application;
using OrderFlow.Notifications.Domain;

namespace OrderFlow.Notifications.UnitTests;

public sealed class NotificationTests
{
    [Fact] public void Create_rejects_missing_recipient() => Assert.Throws<ArgumentException>(() => Notification.Create("", "message", 0));
    [Fact] public async Task Handler_creates_and_persists_notification()
    {
        var repository = new Repository();
        var result = await new CreateHandler(repository).Handle(new CreateNotificationCommand("user@example.test", "message", 0), default);
        Assert.Equal(result.Id, repository.Entity?.Id); Assert.Equal("Pending", result.Status); Assert.NotEmpty(repository.Entity!.DomainEvents);
    }
    private sealed class Repository : INotificationRepository
    {
        public Notification? Entity { get; private set; }
        public Task AddAsync(Notification entity, CancellationToken ct) { Entity = entity; return Task.CompletedTask; }
        public Task<Notification?> GetAsync(Guid id, CancellationToken ct) => Task.FromResult(Entity);
        public Task<IReadOnlyList<Notification>> ListAsync(CancellationToken ct) => Task.FromResult<IReadOnlyList<Notification>>(Entity is null ? [] : [Entity]);
        public Task SaveAsync(CancellationToken ct) => Task.CompletedTask;
        public Task DeleteAsync(Notification entity, CancellationToken ct) => Task.CompletedTask;
    }
}
