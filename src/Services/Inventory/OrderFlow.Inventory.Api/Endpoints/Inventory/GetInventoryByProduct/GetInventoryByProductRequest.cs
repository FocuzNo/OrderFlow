namespace OrderFlow.Inventory.Api.Endpoints;

public sealed record GetInventoryByProductRequest
{
    public Guid ProductId { get; init; }
}
