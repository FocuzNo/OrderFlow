using MediatR;
using OrderFlow.Notifications.Application.Abstractions.Delivery;
using OrderFlow.Notifications.Application.Abstractions.Errors;
using OrderFlow.Notifications.Application.Abstractions.Messaging;
using OrderFlow.Notifications.Application.Abstractions.Persistence;
using OrderFlow.Notifications.Domain.Notifications;

namespace OrderFlow.Notifications.Application.Notifications;

public static partial class NotificationFeatures
{
    public sealed class CreateNotificationCommandHandler(INotificationRepository r)
        : IRequestHandler<CreateNotificationCommand, NotificationResponse>
    {
        public async Task<NotificationResponse> Handle(
            CreateNotificationCommand c,
            CancellationToken ct
        )
        {
            var x = Notification.Create(
                c.Recipient,
                c.Subject,
                c.Body,
                NotificationChannel.FromName(c.Channel, true)
            );
            await r.AddAsync(x, ct);
            return Map(x);
        }
    }
}
