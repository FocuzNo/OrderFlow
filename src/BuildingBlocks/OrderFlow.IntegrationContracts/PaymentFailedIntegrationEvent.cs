namespace OrderFlow.IntegrationContracts;

public sealed record PaymentFailedIntegrationEvent(Guid PaymentId, Guid OrderId, string Reason);
