using F = OrderFlow.Ordering.Application.Orders.OrderFeatures;

namespace OrderFlow.Ordering.Api.Endpoints;

public static partial class OrderEndpoints
{
    public sealed record AddOrderItemRequest
    {
        public Guid Id { get; init; }

        public Guid ProductId { get; init; }

        public string ProductName { get; init; } = string.Empty;

        public decimal UnitPrice { get; init; }

        public int Quantity { get; init; }
    }
}
