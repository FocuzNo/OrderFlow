namespace OrderFlow.Ordering.Api.Endpoints;

public sealed record GetOrdersRequest
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}
