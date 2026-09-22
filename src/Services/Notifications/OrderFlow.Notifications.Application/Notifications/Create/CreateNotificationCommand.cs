using OrderFlow.Notifications.Application.Abstractions.Delivery;
using OrderFlow.Notifications.Application.Abstractions.Errors;
using OrderFlow.Notifications.Application.Abstractions.Messaging;
using OrderFlow.Notifications.Application.Abstractions.Persistence;
using OrderFlow.Notifications.Domain.Notifications;

namespace OrderFlow.Notifications.Application.Notifications;

public static partial class NotificationFeatures
{
    public sealed record CreateNotificationCommand(
        Guid OrderId,
        Guid CustomerId,
        string NotificationType,
        string Recipient,
        string Subject,
        string Body,
        string Channel
    ) : ICommand<NotificationResponse>;
}
