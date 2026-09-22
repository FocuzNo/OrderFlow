namespace OrderFlow.Notifications.Api.Endpoints;

public sealed record GetNotificationsByOrderRequest
{
    public Guid OrderId { get; init; }
}
