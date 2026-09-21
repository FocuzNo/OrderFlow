namespace OrderFlow.IntegrationContracts;

public sealed record NotificationRequestedIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid OrderId,
    string Recipient,
    string Subject,
    string Body
);
