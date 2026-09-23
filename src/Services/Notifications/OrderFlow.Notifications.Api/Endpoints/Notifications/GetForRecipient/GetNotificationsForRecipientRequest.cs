using F = OrderFlow.Notifications.Application.Notifications.NotificationFeatures;

namespace OrderFlow.Notifications.Api.Endpoints;

public static partial class NotificationEndpoints
{
    public sealed record GetNotificationsForRecipientRequest
    {
        public string Recipient { get; init; } = string.Empty;
    }
}
