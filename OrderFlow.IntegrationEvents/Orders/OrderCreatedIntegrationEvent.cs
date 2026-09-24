namespace OrderFlow.IntegrationEvents.Orders;

public sealed record OrderCreatedIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid OrderId,
    IReadOnlyCollection<OrderCreatedItem> Items);
