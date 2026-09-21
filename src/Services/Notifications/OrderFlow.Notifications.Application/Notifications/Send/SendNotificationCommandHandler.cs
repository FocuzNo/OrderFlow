using MediatR;
using OrderFlow.Notifications.Application.Abstractions.Delivery;
using OrderFlow.Notifications.Application.Abstractions.Errors;
using OrderFlow.Notifications.Application.Abstractions.Messaging;
using OrderFlow.Notifications.Application.Abstractions.Persistence;
using OrderFlow.Notifications.Domain.Notifications;

namespace OrderFlow.Notifications.Application.Notifications;

public static partial class NotificationFeatures
{
    public sealed class SendNotificationCommandHandler(
        INotificationRepository r,
        IEmailSender sender
    ) : IRequestHandler<SendNotificationCommand, NotificationResponse>
    {
        public async Task<NotificationResponse> Handle(
            SendNotificationCommand c,
            CancellationToken ct
        )
        {
            var x = await Find(r, c.Id, ct);
            x.StartSending();
            try
            {
                await sender.SendAsync(x.Recipient, x.Subject, x.Body, ct);
                x.RecordSuccess();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                x.RecordFailure(ex.Message);
            }
            await r.SaveAsync(ct);
            return Map(x);
        }
    }
}
