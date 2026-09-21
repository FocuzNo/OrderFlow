namespace OrderFlow.IntegrationContracts;

public sealed record OrderCancelledIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid OrderId,
    string CustomerEmail,
    string Reason
);
