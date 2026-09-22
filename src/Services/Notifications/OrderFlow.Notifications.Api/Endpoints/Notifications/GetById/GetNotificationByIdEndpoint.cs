using F = OrderFlow.Notifications.Application.Notifications.NotificationFeatures;

namespace OrderFlow.Notifications.Api.Endpoints;

public static partial class NotificationEndpoints
{
    public sealed class GetNotificationByIdEndpoint(ISender sender)
        : Endpoint<NotificationIdRequest, F.NotificationResponse>
    {
        public override void Configure()
        {
            Get("/api/notifications/{id}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            NotificationIdRequest request,
            CancellationToken cancellationToken
        ) =>
            await Send.OkAsync(
                await sender.Send(new F.GetNotificationByIdQuery(request.Id), cancellationToken),
                cancellationToken
            );
    }
}
