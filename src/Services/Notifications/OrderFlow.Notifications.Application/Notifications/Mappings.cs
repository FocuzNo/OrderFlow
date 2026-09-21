using OrderFlow.Notifications.Application.Abstractions.Delivery;
using OrderFlow.Notifications.Application.Abstractions.Errors;
using OrderFlow.Notifications.Application.Abstractions.Messaging;
using OrderFlow.Notifications.Application.Abstractions.Persistence;
using OrderFlow.Notifications.Domain.Notifications;

namespace OrderFlow.Notifications.Application.Notifications;

public static partial class NotificationFeatures
{
    private static async Task<Notification> Find(
        INotificationRepository repository,
        Guid id,
        CancellationToken cancellationToken
    ) =>
        await repository.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException("Notification was not found.");

    private static NotificationResponse Map(Notification entity) =>
        new(
            entity.Id,
            entity.Recipient,
            entity.Subject,
            entity.Body,
            entity.Channel.Name,
            entity.Status.Name,
            entity
                .Attempts.Select(attempt => new NotificationAttemptResponse(
                    attempt.Id,
                    attempt.Succeeded,
                    attempt.Error,
                    attempt.AttemptedAt
                ))
                .ToArray(),
            entity.CreatedAt,
            entity.SentAt
        );
}
