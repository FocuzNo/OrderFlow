using OrderFlow.Notifications.Application.Abstractions.Messaging;

namespace OrderFlow.Notifications.Application.Notifications;

public static partial class NotificationFeatures
{
    public sealed record GetNotificationsByOrderQuery(Guid OrderId)
        : IQuery<IReadOnlyList<NotificationResponse>>;
}
