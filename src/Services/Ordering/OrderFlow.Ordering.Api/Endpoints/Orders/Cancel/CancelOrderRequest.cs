using F = OrderFlow.Ordering.Application.Orders.OrderFeatures;

namespace OrderFlow.Ordering.Api.Endpoints;

public static partial class OrderEndpoints
{
    public sealed record CancelOrderRequest
    {
        public Guid Id { get; init; }

        public string Reason { get; init; } = string.Empty;
    }
}
