namespace OrderFlow.Inventory.Api.Endpoints;

public sealed record DeleteInventoryRequest
{
    public Guid ProductId { get; init; }
}
