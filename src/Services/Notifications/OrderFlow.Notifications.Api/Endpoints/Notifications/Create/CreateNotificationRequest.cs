using F = OrderFlow.Notifications.Application.Notifications.NotificationFeatures;

namespace OrderFlow.Notifications.Api.Endpoints;

public static partial class NotificationEndpoints
{
    public sealed record CreateNotificationRequest
    {
        public string Recipient { get; init; } = string.Empty;

        public string Subject { get; init; } = string.Empty;

        public string Body { get; init; } = string.Empty;

        public string Channel { get; init; } = "Email";
    }
}
