namespace OrderFlow.Notifications.Application.Notifications;

public static partial class NotificationFeatures
{
    public sealed class RetryNotificationCommandValidator
        : AbstractValidator<RetryNotificationCommand>
    {
        public RetryNotificationCommandValidator() => RuleFor(x => x.Id).NotEmpty();
    }
}
