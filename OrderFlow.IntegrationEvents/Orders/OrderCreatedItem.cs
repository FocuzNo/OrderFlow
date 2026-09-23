namespace OrderFlow.IntegrationEvents.Orders;

public sealed record OrderCreatedItem(
    Guid ProductId,
    int Quantity);
