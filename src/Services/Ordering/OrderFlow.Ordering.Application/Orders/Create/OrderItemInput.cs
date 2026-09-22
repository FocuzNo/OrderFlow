namespace OrderFlow.Ordering.Application.Orders;

public static partial class OrderFeatures
{
    public sealed record OrderItemInput(
        Guid ProductId,
        string ProductName,
        decimal UnitPrice,
        int Quantity
    );
}
