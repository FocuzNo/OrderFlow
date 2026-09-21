using F = OrderFlow.Inventory.Application.Inventory.InventoryFeatures;

namespace OrderFlow.Inventory.Api.Endpoints;

public static partial class InventoryEndpoints
{
    public sealed record ChangeReservationStateRequest
    {
        public Guid ProductId { get; init; }

        public Guid WarehouseId { get; init; }

        public Guid ReservationId { get; init; }
    }
}
