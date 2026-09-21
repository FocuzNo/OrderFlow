namespace OrderFlow.IntegrationContracts;

public sealed record PaymentRequestedIntegrationEvent(Guid OrderId, decimal Amount);
