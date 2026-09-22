namespace OrderFlow.Notifications.Application.Notifications;

public static partial class NotificationFeatures
{
    public sealed class RetryNotificationCommandValidator
        : AbstractValidator<RetryNotificationCommand>
    {
        public RetryNotificationCommandValidator() => RuleFor(candidate => candidate.Id).NotEmpty();
    }
}
