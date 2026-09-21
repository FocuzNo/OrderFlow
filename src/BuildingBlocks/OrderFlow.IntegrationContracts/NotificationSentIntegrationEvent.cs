namespace OrderFlow.IntegrationContracts;

public sealed record NotificationSentIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid NotificationId,
    string Recipient
);
