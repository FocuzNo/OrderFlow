using OrderFlow.Notifications.Domain.Notifications;

namespace OrderFlow.Notifications.Application.Abstractions.Persistence;

public interface INotificationRepository : IRepository<Notification>
{
    Task<IReadOnlyList<Notification>> GetByOrderAsync(
        Guid orderId,
        CancellationToken cancellationToken
    );
    Task<IReadOnlyList<Notification>> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken
    );
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Notification>> GetForRecipientAsync(
        string recipient,
        CancellationToken cancellationToken
    );
}
