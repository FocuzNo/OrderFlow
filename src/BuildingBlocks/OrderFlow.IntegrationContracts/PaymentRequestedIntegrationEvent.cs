namespace OrderFlow.IntegrationContracts;

public sealed record PaymentRequestedIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid OrderId,
    decimal Amount
);
