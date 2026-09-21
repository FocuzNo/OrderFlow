using FastEndpoints;
using MediatR;
using F = OrderFlow.Notifications.Application.Notifications.NotificationFeatures;

namespace OrderFlow.Notifications.Api.Endpoints;

public static partial class NotificationEndpoints
{
    public sealed class GetNotificationsForRecipientEndpoint(ISender s)
        : Endpoint<GetNotificationsForRecipientRequest, IReadOnlyList<F.NotificationResponse>>
    {
        public override void Configure()
        {
            Get("/api/notifications");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            GetNotificationsForRecipientRequest r,
            CancellationToken ct
        ) =>
            await Send.OkAsync(
                await s.Send(new F.GetNotificationsForRecipientQuery(r.Recipient), ct),
                ct
            );
    }
}
