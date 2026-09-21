namespace OrderFlow.IntegrationContracts;

public sealed record OrderConfirmedIntegrationEvent(
    Guid OrderId,
    Guid CustomerId,
    string CustomerEmail
);
