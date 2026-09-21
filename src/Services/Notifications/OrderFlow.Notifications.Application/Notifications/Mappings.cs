using MediatR;
using OrderFlow.Notifications.Application.Abstractions.Delivery;
using OrderFlow.Notifications.Application.Abstractions.Errors;
using OrderFlow.Notifications.Application.Abstractions.Messaging;
using OrderFlow.Notifications.Application.Abstractions.Persistence;
using OrderFlow.Notifications.Domain.Notifications;

namespace OrderFlow.Notifications.Application.Notifications;

public static partial class NotificationFeatures
{
    private static async Task<Notification> Find(
        INotificationRepository r,
        Guid id,
        CancellationToken ct
    ) => await r.GetAsync(id, ct) ?? throw new NotFoundException("Notification was not found.");

    private static NotificationResponse Map(Notification x) =>
        new(
            x.Id,
            x.Recipient,
            x.Subject,
            x.Body,
            x.Channel.Name,
            x.Status.Name,
            x.Attempts.Select(a => new NotificationAttemptResponse(
                    a.Id,
                    a.Succeeded,
                    a.Error,
                    a.AttemptedAt
                ))
                .ToArray(),
            x.CreatedAt,
            x.SentAt
        );
}
