namespace OrderFlow.IntegrationContracts;

public sealed record OrderConfirmedIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid OrderId,
    Guid CustomerId,
    string CustomerEmail
);
