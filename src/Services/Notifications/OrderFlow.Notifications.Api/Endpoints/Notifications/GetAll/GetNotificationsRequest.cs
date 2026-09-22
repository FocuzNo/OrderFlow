namespace OrderFlow.Notifications.Api.Endpoints;

public sealed record GetNotificationsRequest
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}
