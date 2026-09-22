namespace OrderFlow.Notifications.Application.Notifications;

public sealed class CreateNotificationCommandValidator
    : AbstractValidator<NotificationFeatures.CreateNotificationCommand>
{
    public CreateNotificationCommandValidator()
    {
        RuleFor(command => command.OrderId).NotEmpty();
        RuleFor(command => command.CustomerId).NotEmpty();
        RuleFor(command => command.NotificationType).NotEmpty().MaximumLength(100);
        RuleFor(candidate => candidate.Recipient).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(candidate => candidate.Subject).NotEmpty().MaximumLength(250);
        RuleFor(candidate => candidate.Body).NotEmpty().MaximumLength(10000);
        RuleFor(candidate => candidate.Channel).Equal("Email", StringComparer.OrdinalIgnoreCase);
    }
}
