using F = OrderFlow.Notifications.Application.Notifications.NotificationFeatures;

namespace OrderFlow.Notifications.Api.Endpoints;

public static partial class NotificationEndpoints
{
    public sealed class SendNotificationEndpoint(ISender sender)
        : Endpoint<NotificationIdRequest, F.NotificationResponse>
    {
        public override void Configure()
        {
            Post("/api/notifications/{id}/send");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            NotificationIdRequest request,
            CancellationToken cancellationToken
        ) =>
            await Send.OkAsync(
                await sender.Send(new F.SendNotificationCommand(request.Id), cancellationToken),
                cancellationToken
            );
    }
}
