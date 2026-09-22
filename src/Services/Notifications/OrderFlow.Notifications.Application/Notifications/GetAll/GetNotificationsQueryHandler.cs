using OrderFlow.Notifications.Application.Abstractions.Persistence;

namespace OrderFlow.Notifications.Application.Notifications;

public static partial class NotificationFeatures
{
    public sealed class GetNotificationsQueryHandler(INotificationRepository repository)
        : IRequestHandler<GetNotificationsQuery, IReadOnlyList<NotificationResponse>>
    {
        public async Task<IReadOnlyList<NotificationResponse>> Handle(
            GetNotificationsQuery query,
            CancellationToken cancellationToken
        ) =>
            (await repository.ListAsync(query.Page, query.PageSize, cancellationToken))
                .Select(Map)
                .ToArray();
    }
}
