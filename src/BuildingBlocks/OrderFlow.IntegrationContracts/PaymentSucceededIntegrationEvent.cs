namespace OrderFlow.IntegrationContracts;

public sealed record PaymentSucceededIntegrationEvent(Guid PaymentId, Guid OrderId, decimal Amount);
