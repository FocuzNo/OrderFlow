using F = OrderFlow.Notifications.Application.Notifications.NotificationFeatures;

namespace OrderFlow.Notifications.Api.Endpoints;

public static partial class NotificationEndpoints
{
    public sealed class RetryNotificationEndpoint(ISender sender)
        : Endpoint<NotificationIdRequest, F.NotificationResponse>
    {
        public override void Configure()
        {
            Post("/api/notifications/{id}/retry");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            NotificationIdRequest request,
            CancellationToken cancellationToken
        ) =>
            await Send.OkAsync(
                await sender.Send(new F.RetryNotificationCommand(request.Id), cancellationToken),
                cancellationToken
            );
    }
}
