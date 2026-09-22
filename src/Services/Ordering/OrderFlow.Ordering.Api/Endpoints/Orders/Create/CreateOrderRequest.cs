using F = OrderFlow.Ordering.Application.Orders.OrderFeatures;

namespace OrderFlow.Ordering.Api.Endpoints;

public static partial class OrderEndpoints
{
    public sealed record CreateOrderRequest
    {
        public IReadOnlyList<CreateOrderItemRequest> Items { get; init; } = [];

        public Guid CustomerId { get; init; }

        public string CustomerEmail { get; init; } = string.Empty;

        public string Line1 { get; init; } = string.Empty;

        public string City { get; init; } = string.Empty;

        public string PostalCode { get; init; } = string.Empty;

        public string Country { get; init; } = string.Empty;
    }
}
