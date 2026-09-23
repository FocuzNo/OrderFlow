using F = OrderFlow.Ordering.Application.Orders.OrderFeatures;

namespace OrderFlow.Ordering.Api.Endpoints;

public static partial class OrderEndpoints
{
    public sealed record RemoveOrderItemRequest
    {
        public Guid Id { get; init; }

        public Guid ItemId { get; init; }
    }
}
