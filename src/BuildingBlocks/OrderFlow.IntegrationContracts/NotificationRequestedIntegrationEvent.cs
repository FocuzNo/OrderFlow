namespace OrderFlow.IntegrationContracts;

public sealed record NotificationRequestedIntegrationEvent(Guid OrderId, string Recipient, string Subject, string Body);
