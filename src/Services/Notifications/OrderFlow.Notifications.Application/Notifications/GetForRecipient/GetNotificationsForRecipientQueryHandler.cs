using OrderFlow.Notifications.Application.Abstractions.Delivery;
using OrderFlow.Notifications.Application.Abstractions.Errors;
using OrderFlow.Notifications.Application.Abstractions.Messaging;
using OrderFlow.Notifications.Application.Abstractions.Persistence;
using OrderFlow.Notifications.Domain.Notifications;

namespace OrderFlow.Notifications.Application.Notifications;

public static partial class NotificationFeatures
{
    public sealed class GetNotificationsForRecipientQueryHandler(INotificationRepository repository)
        : IRequestHandler<GetNotificationsForRecipientQuery, IReadOnlyList<NotificationResponse>>
    {
        public async Task<IReadOnlyList<NotificationResponse>> Handle(
            GetNotificationsForRecipientQuery q,
            CancellationToken cancellationToken
        ) =>
            (await repository.GetForRecipientAsync(q.Recipient, cancellationToken))
                .Select(Map)
                .ToArray();
    }
}
