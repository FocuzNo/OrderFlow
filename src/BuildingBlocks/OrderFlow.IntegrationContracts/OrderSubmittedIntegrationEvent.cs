namespace OrderFlow.IntegrationContracts;

public sealed record OrderSubmittedIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid OrderId,
    Guid CustomerId,
    string CustomerEmail,
    IReadOnlyList<OrderItemContract> Items,
    decimal TotalAmount
);
