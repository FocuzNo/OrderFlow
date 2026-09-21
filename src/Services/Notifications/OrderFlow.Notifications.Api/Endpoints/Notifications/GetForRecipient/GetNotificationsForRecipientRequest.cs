using FastEndpoints;
using MediatR;
using F = OrderFlow.Notifications.Application.Notifications.NotificationFeatures;

namespace OrderFlow.Notifications.Api.Endpoints;

public static partial class NotificationEndpoints
{
    public sealed class GetNotificationsForRecipientRequest
    {
        public string Recipient { get; set; } = string.Empty;
    }
}
