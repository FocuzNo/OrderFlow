namespace OrderFlow.Ordering.Domain.Orders;
public sealed record OrderItemSnapshot(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);
