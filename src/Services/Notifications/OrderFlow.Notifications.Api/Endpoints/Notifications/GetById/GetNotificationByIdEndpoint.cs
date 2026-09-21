using FastEndpoints;
using MediatR;
using F = OrderFlow.Notifications.Application.Notifications.NotificationFeatures;

namespace OrderFlow.Notifications.Api.Endpoints;

public static partial class NotificationEndpoints
{
    public sealed class GetNotificationByIdEndpoint(ISender s)
        : Endpoint<NotificationIdRequest, F.NotificationResponse>
    {
        public override void Configure()
        {
            Get("/api/notifications/{id}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(NotificationIdRequest r, CancellationToken ct) =>
            await Send.OkAsync(await s.Send(new F.GetNotificationByIdQuery(r.Id), ct), ct);
    }
}
