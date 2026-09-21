namespace OrderFlow.IntegrationContracts;

public sealed record OrderItemContract(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);
