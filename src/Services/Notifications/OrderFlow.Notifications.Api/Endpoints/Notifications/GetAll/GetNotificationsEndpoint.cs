using F = OrderFlow.Notifications.Application.Notifications.NotificationFeatures;

namespace OrderFlow.Notifications.Api.Endpoints;

public sealed class GetNotificationsEndpoint(ISender sender)
    : Endpoint<GetNotificationsRequest, IReadOnlyList<F.NotificationResponse>>
{
    public override void Configure()
    {
        Get("/api/notifications");
        AllowAnonymous();
    }

    public override async Task HandleAsync(
        GetNotificationsRequest request,
        CancellationToken cancellationToken
    ) =>
        await Send.OkAsync(
            await sender.Send(
                new F.GetNotificationsQuery(request.Page, request.PageSize),
                cancellationToken
            ),
            cancellationToken
        );
}
