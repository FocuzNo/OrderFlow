using static OrderFlow.Notifications.Application.Notifications.NotificationFeatures;

namespace OrderFlow.Notifications.Api.Endpoints;

public sealed class GetNotificationsByOrderEndpoint(ISender sender)
    : Endpoint<GetNotificationsByOrderRequest, IReadOnlyList<NotificationResponse>>
{
    public override void Configure()
    {
        Get("/api/notifications/order/{orderId}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(
        GetNotificationsByOrderRequest request,
        CancellationToken cancellationToken
    ) =>
        await Send.OkAsync(
            await sender.Send(new GetNotificationsByOrderQuery(request.OrderId), cancellationToken),
            cancellationToken
        );
}
