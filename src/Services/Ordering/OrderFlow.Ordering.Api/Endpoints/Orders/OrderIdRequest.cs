using F = OrderFlow.Ordering.Application.Orders.OrderFeatures;

namespace OrderFlow.Ordering.Api.Endpoints;

public static partial class OrderEndpoints
{
    public sealed record OrderIdRequest
    {
        public Guid Id { get; init; }
    }
}
