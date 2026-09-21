using OrderFlow.Notifications.Domain.Notifications;

namespace OrderFlow.Notifications.Application.Abstractions.Persistence;

public interface INotificationRepository
{
    Task<Notification?> GetAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Notification>> GetForRecipientAsync(string recipient, CancellationToken ct);
    Task AddAsync(Notification notification, CancellationToken ct);
    Task SaveAsync(CancellationToken ct);
}
