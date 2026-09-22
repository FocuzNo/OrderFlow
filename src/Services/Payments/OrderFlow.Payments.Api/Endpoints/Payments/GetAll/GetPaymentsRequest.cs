namespace OrderFlow.Payments.Api.Endpoints;

public sealed record GetPaymentsRequest
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}
