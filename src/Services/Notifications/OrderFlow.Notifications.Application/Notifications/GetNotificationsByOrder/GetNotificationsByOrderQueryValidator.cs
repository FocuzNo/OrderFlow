namespace OrderFlow.Notifications.Application.Notifications;

public static partial class NotificationFeatures
{
    public sealed class GetNotificationsByOrderQueryValidator
        : AbstractValidator<GetNotificationsByOrderQuery>
    {
        public GetNotificationsByOrderQueryValidator()
        {
            RuleFor(query => query.OrderId).NotEmpty();
        }
    }
}
