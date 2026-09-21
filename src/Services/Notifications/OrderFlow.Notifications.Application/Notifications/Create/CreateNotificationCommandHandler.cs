using OrderFlow.Notifications.Application.Abstractions.Delivery;
using OrderFlow.Notifications.Application.Abstractions.Errors;
using OrderFlow.Notifications.Application.Abstractions.Messaging;
using OrderFlow.Notifications.Application.Abstractions.Persistence;
using OrderFlow.Notifications.Domain.Notifications;

namespace OrderFlow.Notifications.Application.Notifications;

public static partial class NotificationFeatures
{
    public sealed class CreateNotificationCommandHandler(
        INotificationRepository repository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<CreateNotificationCommand, NotificationResponse>
    {
        public async Task<NotificationResponse> Handle(
            CreateNotificationCommand command,
            CancellationToken cancellationToken
        )
        {
            var entity = Notification.Create(
                command.Recipient,
                command.Subject,
                command.Body,
                NotificationChannel.FromName(command.Channel, true)
            );
            await repository.AddAsync(entity, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Map(entity);
        }
    }
}
