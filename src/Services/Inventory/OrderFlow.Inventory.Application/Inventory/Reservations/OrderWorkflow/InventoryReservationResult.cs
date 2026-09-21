namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed record InventoryReservationResult(
        bool Succeeded,
        IReadOnlyList<Guid> ReservationIds,
        string? Error
    );
}
