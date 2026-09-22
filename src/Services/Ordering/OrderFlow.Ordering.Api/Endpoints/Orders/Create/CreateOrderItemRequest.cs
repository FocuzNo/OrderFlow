namespace OrderFlow.Ordering.Api.Endpoints;

public static partial class OrderEndpoints
{
    public sealed record CreateOrderItemRequest(
        Guid ProductId,
        string ProductName,
        decimal UnitPrice,
        int Quantity
    );
}
