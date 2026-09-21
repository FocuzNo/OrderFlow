using OrderFlow.Notifications.Domain.Notifications;

namespace OrderFlow.Notifications.Application.Abstractions.Persistence;

public interface INotificationRepository : IRepository<Notification>
{
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Notification>> GetForRecipientAsync(
        string recipient,
        CancellationToken cancellationToken
    );
}
