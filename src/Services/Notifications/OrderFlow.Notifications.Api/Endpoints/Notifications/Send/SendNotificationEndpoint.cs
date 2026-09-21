using FastEndpoints;
using MediatR;
using F = OrderFlow.Notifications.Application.Notifications.NotificationFeatures;

namespace OrderFlow.Notifications.Api.Endpoints;

public static partial class NotificationEndpoints
{
    public sealed class SendNotificationEndpoint(ISender s)
        : Endpoint<NotificationIdRequest, F.NotificationResponse>
    {
        public override void Configure()
        {
            Post("/api/notifications/{id}/send");
            AllowAnonymous();
        }

        public override async Task HandleAsync(NotificationIdRequest r, CancellationToken ct) =>
            await Send.OkAsync(await s.Send(new F.SendNotificationCommand(r.Id), ct), ct);
    }
}
