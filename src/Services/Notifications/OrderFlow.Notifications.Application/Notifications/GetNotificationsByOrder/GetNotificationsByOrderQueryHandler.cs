using OrderFlow.Notifications.Application.Abstractions.Errors;
using OrderFlow.Notifications.Application.Abstractions.Persistence;

namespace OrderFlow.Notifications.Application.Notifications;

public static partial class NotificationFeatures
{
    public sealed class GetNotificationsByOrderQueryHandler(INotificationRepository repository)
        : IRequestHandler<GetNotificationsByOrderQuery, IReadOnlyList<NotificationResponse>>
    {
        public async Task<IReadOnlyList<NotificationResponse>> Handle(
            GetNotificationsByOrderQuery query,
            CancellationToken cancellationToken
        ) =>
            (await repository.GetByOrderAsync(query.OrderId, cancellationToken))
                .Select(Map)
                .ToArray();
    }
}
