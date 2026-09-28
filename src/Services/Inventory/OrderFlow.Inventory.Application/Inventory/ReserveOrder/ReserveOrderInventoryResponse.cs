namespace OrderFlow.Inventory.Application.Inventory;

public sealed record ReserveOrderInventoryResponse(
    bool IsDuplicate,
    bool IsReserved,
    string? FailureReason,
    IReadOnlyCollection<ReservedStockItem> Items
);
