using MediatR;
using OrderFlow.Notifications.Application.Abstractions.Delivery;
using OrderFlow.Notifications.Application.Abstractions.Errors;
using OrderFlow.Notifications.Application.Abstractions.Messaging;
using OrderFlow.Notifications.Application.Abstractions.Persistence;
using OrderFlow.Notifications.Domain.Notifications;

namespace OrderFlow.Notifications.Application.Notifications;

public static partial class NotificationFeatures
{
    public sealed class RetryNotificationCommandHandler(IMediator mediator)
        : IRequestHandler<RetryNotificationCommand, NotificationResponse>
    {
        public Task<NotificationResponse> Handle(
            RetryNotificationCommand c,
            CancellationToken ct
        ) => mediator.Send(new SendNotificationCommand(c.Id), ct);
    }
}
