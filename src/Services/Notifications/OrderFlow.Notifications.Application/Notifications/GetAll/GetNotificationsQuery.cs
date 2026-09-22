using OrderFlow.Notifications.Application.Abstractions.Messaging;

namespace OrderFlow.Notifications.Application.Notifications;

public static partial class NotificationFeatures
{
    public sealed record GetNotificationsQuery(int Page = 1, int PageSize = 20)
        : IQuery<IReadOnlyList<NotificationResponse>>;
}
