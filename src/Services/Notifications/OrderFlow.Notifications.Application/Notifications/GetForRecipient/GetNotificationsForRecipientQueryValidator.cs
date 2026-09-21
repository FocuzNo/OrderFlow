namespace OrderFlow.Notifications.Application.Notifications;

public static partial class NotificationFeatures
{
    public sealed class GetNotificationsForRecipientQueryValidator
        : AbstractValidator<GetNotificationsForRecipientQuery>
    {
        public GetNotificationsForRecipientQueryValidator() =>
            RuleFor(query => query.Recipient).NotEmpty().EmailAddress();
    }
}
