using F = OrderFlow.Notifications.Application.Notifications.NotificationFeatures;

namespace OrderFlow.Notifications.Api.Endpoints;

public static partial class NotificationEndpoints
{
    public sealed class GetNotificationsForRecipientEndpoint(ISender sender)
        : Endpoint<GetNotificationsForRecipientRequest, IReadOnlyList<F.NotificationResponse>>
    {
        public override void Configure()
        {
            Get("/api/notifications/by-recipient");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            GetNotificationsForRecipientRequest request,
            CancellationToken cancellationToken
        ) =>
            await Send.OkAsync(
                await sender.Send(
                    new F.GetNotificationsForRecipientQuery(request.Recipient),
                    cancellationToken
                ),
                cancellationToken
            );
    }
}
