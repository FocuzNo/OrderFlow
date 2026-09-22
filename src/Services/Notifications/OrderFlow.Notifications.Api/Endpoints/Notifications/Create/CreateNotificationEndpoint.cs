using F = OrderFlow.Notifications.Application.Notifications.NotificationFeatures;

namespace OrderFlow.Notifications.Api.Endpoints;

public static partial class NotificationEndpoints
{
    public sealed class CreateNotificationEndpoint(ISender sender)
        : Endpoint<CreateNotificationRequest, F.NotificationResponse>
    {
        public override void Configure()
        {
            Post("/api/notifications");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            CreateNotificationRequest request,
            CancellationToken cancellationToken
        ) =>
            await Send.ResponseAsync(
                await sender.Send(
                    new F.CreateNotificationCommand(
                        request.OrderId,
                        request.CustomerId,
                        request.NotificationType,
                        request.Recipient,
                        request.Subject,
                        request.Body,
                        request.Channel
                    ),
                    cancellationToken
                ),
                201,
                cancellationToken
            );
    }
}
