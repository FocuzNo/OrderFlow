using Microsoft.Extensions.Logging.Abstractions;
using OrderFlow.Notifications.Application.Abstractions.Persistence;
using OrderFlow.Notifications.Application.Notifications;
using OrderFlow.Notifications.Domain.Notifications;

namespace OrderFlow.Notifications.UnitTests;

public sealed class CreateNotificationTests
{
    [Fact]
    public async Task Create_ShouldRecordOrderHistoryAndCommitOnce()
    {
        var repository = new Repository();
        var commit = new Commit();
        var command = new NotificationFeatures.CreateNotificationCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "OrderRecorded",
            "user@example.test",
            "Subject",
            "Message",
            "Email"
        );
        var handler = new NotificationFeatures.CreateNotificationCommandHandler(
            repository,
            commit,
            NullLogger<NotificationFeatures.CreateNotificationCommandHandler>.Instance
        );
        var response = await handler.Handle(command, default);
        Assert.Equal(command.OrderId, response.OrderId);
        Assert.Equal(1, commit.Count);
        Assert.NotNull(repository.Entity);
    }

    [Fact]
    public void Validator_ShouldRejectMissingOrder() =>
        Assert.False(
            new CreateNotificationCommandValidator()
                .Validate(
                    new NotificationFeatures.CreateNotificationCommand(
                        Guid.Empty,
                        Guid.NewGuid(),
                        "OrderRecorded",
                        "user@example.test",
                        "Subject",
                        "Message",
                        "Email"
                    )
                )
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

    private sealed class Repository : INotificationRepository
    {
        public Notification? Entity { get; private set; }

        public Task AddAsync(Notification entity, CancellationToken cancellationToken)
        {
            Entity = entity;
            return Task.CompletedTask;
        }

        public Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Entity);

        public Task<IReadOnlyList<Notification>> ListAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken
        ) => Task.FromResult<IReadOnlyList<Notification>>([]);

        public Task<IReadOnlyList<Notification>> GetByOrderAsync(
            Guid orderId,
            CancellationToken cancellationToken
        ) => Task.FromResult<IReadOnlyList<Notification>>([]);

        public Task<IReadOnlyList<Notification>> GetForRecipientAsync(
            string recipient,
            CancellationToken cancellationToken
        ) => Task.FromResult<IReadOnlyList<Notification>>([]);
    }
}
