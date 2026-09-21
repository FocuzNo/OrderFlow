namespace OrderFlow.IntegrationContracts;

public sealed record NotificationSentIntegrationEvent(Guid NotificationId, string Recipient);
