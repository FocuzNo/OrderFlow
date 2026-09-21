using FluentValidation;

namespace OrderFlow.Notifications.Application.Notifications;

public sealed class CreateNotificationValidator : AbstractValidator<NotificationFeatures.Create>
{
    public CreateNotificationValidator()
    {
        RuleFor(x => x.Recipient).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Body).NotEmpty().MaximumLength(10000);
        RuleFor(x => x.Channel).Equal("Email", StringComparer.OrdinalIgnoreCase);
    }
}
