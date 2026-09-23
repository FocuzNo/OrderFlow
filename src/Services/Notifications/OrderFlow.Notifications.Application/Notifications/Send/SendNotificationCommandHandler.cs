using OrderFlow.Notifications.Application.Abstractions.Delivery;
using OrderFlow.Notifications.Application.Abstractions.Errors;
using OrderFlow.Notifications.Application.Abstractions.Messaging;
using OrderFlow.Notifications.Application.Abstractions.Persistence;
using OrderFlow.Notifications.Domain.Notifications;

namespace OrderFlow.Notifications.Application.Notifications;

public static partial class NotificationFeatures
{
    public sealed class SendNotificationCommandHandler(
        INotificationRepository repository,
        IEmailSender sender,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<SendNotificationCommand, NotificationResponse>
    {
        public async Task<NotificationResponse> Handle(
            SendNotificationCommand command,
            CancellationToken cancellationToken
        )
        {
            var entity = await Find(repository, command.Id, cancellationToken);
            entity.StartSending();
            try
            {
                await sender.SendAsync(
                    entity.Recipient,
                    entity.Subject,
                    entity.Body,
                    cancellationToken
                );
                entity.RecordSuccess();
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                entity.RecordFailure(exception.Message);
            }
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Map(entity);
        }
    }
}
