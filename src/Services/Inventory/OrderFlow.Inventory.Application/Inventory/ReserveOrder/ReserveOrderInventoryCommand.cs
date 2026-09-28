namespace OrderFlow.Inventory.Application.Inventory;

public sealed record ReserveOrderInventoryCommand(
    Guid EventId,
    Guid OrderId,
    IReadOnlyCollection<ReserveOrderInventoryItem> Items
) : IRequest<ReserveOrderInventoryResponse>;
