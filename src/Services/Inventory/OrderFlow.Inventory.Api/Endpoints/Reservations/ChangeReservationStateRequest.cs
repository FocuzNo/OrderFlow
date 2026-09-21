using FastEndpoints;
using MediatR;
using F = OrderFlow.Inventory.Application.Inventory.InventoryFeatures;

namespace OrderFlow.Inventory.Api.Endpoints;

public static partial class InventoryEndpoints
{
    public sealed class ChangeReservationStateRequest
    {
        public Guid ProductId { get; set; }

        public Guid WarehouseId { get; set; }

        public Guid ReservationId { get; set; }
    }
}
