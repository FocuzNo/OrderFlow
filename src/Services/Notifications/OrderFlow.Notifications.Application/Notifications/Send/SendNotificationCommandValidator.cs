namespace OrderFlow.Notifications.Application.Notifications;

public static partial class NotificationFeatures
{
    public sealed class SendNotificationCommandValidator
        : AbstractValidator<SendNotificationCommand>
    {
        public SendNotificationCommandValidator() => RuleFor(candidate => candidate.Id).NotEmpty();
    }
}
