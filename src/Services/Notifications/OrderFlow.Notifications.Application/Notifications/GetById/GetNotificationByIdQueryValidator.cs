namespace OrderFlow.Notifications.Application.Notifications;

public static partial class NotificationFeatures
{
    public sealed class GetNotificationByIdQueryValidator
        : AbstractValidator<GetNotificationByIdQuery>
    {
        public GetNotificationByIdQueryValidator() => RuleFor(query => query.Id).NotEmpty();
    }
}
