using FastEndpoints;
using MediatR;
using F = OrderFlow.Notifications.Application.Notifications.NotificationFeatures;

namespace OrderFlow.Notifications.Api.Endpoints;

public static partial class NotificationEndpoints
{
    public sealed class CreateNotificationEndpoint(ISender s)
        : Endpoint<CreateNotificationRequest, F.NotificationResponse>
    {
        public override void Configure()
        {
            Post("/api/notifications");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CreateNotificationRequest r, CancellationToken ct) =>
            await Send.ResponseAsync(
                await s.Send(
                    new F.CreateNotificationCommand(r.Recipient, r.Subject, r.Body, r.Channel),
                    ct
                ),
                201,
                ct
            );
    }
}
