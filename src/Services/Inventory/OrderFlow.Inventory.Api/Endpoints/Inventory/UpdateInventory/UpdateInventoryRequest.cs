namespace OrderFlow.Inventory.Api.Endpoints;

public sealed record UpdateInventoryRequest
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}
