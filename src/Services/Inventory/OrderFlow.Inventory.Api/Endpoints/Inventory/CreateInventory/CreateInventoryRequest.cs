namespace OrderFlow.Inventory.Api.Endpoints;

public sealed record CreateInventoryRequest
{
    public Guid ProductId { get; init; }
    public Guid WarehouseId { get; init; }
    public string Sku { get; init; } = string.Empty;
    public int Quantity { get; init; }
}
