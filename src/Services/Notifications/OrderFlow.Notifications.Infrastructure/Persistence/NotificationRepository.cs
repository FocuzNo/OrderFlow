using OrderFlow.Notifications.Application.Abstractions.Persistence;
using OrderFlow.Notifications.Domain.Notifications;
using OrderFlow.Notifications.Infrastructure.Persistence.Repositories;

namespace OrderFlow.Notifications.Infrastructure.Persistence;

public sealed class NotificationRepository(NotificationsDbContext databaseContext)
    : Repository<Notification>(databaseContext),
        INotificationRepository
{
    public async Task<IReadOnlyList<Notification>> GetByOrderAsync(
        Guid orderId,
        CancellationToken cancellationToken
    ) =>
        await DatabaseContext
            .Notifications.AsNoTracking()
            .Include(notification => notification.Attempts)
            .Where(notification => notification.OrderId == orderId)
            .OrderBy(notification => notification.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Notification>> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken
    ) =>
        await DatabaseContext
            .Notifications.AsNoTracking()
            .Include(notification => notification.Attempts)
            .OrderBy(entity => entity.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        DatabaseContext
            .Notifications.Include(candidate => candidate.Attempts)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Notification>> GetForRecipientAsync(
        string recipient,
        CancellationToken cancellationToken
    ) =>
        await DatabaseContext
            .Notifications.AsNoTracking()
            .Include(candidate => candidate.Attempts)
            .Where(candidate => candidate.Recipient == recipient)
            .OrderByDescending(candidate => candidate.CreatedAt)
            .ToListAsync(cancellationToken);
}
