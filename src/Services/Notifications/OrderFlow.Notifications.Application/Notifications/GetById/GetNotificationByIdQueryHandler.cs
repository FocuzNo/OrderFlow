using OrderFlow.Notifications.Application.Abstractions.Delivery;
using OrderFlow.Notifications.Application.Abstractions.Errors;
using OrderFlow.Notifications.Application.Abstractions.Messaging;
using OrderFlow.Notifications.Application.Abstractions.Persistence;
using OrderFlow.Notifications.Domain.Notifications;

namespace OrderFlow.Notifications.Application.Notifications;

public static partial class NotificationFeatures
{
    public sealed class GetNotificationByIdQueryHandler(INotificationRepository repository)
        : IRequestHandler<GetNotificationByIdQuery, NotificationResponse>
    {
        public async Task<NotificationResponse> Handle(
            GetNotificationByIdQuery q,
            CancellationToken cancellationToken
        ) => Map(await Find(repository, q.Id, cancellationToken));
    }
}
