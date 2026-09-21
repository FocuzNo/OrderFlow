namespace OrderFlow.IntegrationContracts;

public sealed record PaymentFailedIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid PaymentId,
    Guid OrderId,
    string Reason
);
