namespace OrderFlow.IntegrationContracts;

public sealed record OrderCancelledIntegrationEvent(
    Guid OrderId,
    string CustomerEmail,
    string Reason
);
