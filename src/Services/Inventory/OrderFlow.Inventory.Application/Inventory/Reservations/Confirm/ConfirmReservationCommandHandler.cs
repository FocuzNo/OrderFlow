using MediatR;
using OrderFlow.Inventory.Application.Abstractions.Errors;
using OrderFlow.Inventory.Application.Abstractions.Messaging;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Stock;
using OrderFlow.Inventory.Domain.Warehouses;

namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class ConfirmReservationCommandHandler(IInventoryRepository r)
        : IRequestHandler<ConfirmReservationCommand>
    {
        public async Task Handle(ConfirmReservationCommand c, CancellationToken ct)
        {
            var x = await Find(r, c.ProductId, c.WarehouseId, ct);
            x.Confirm(c.ReservationId);
            await r.SaveAsync(ct);
        }
    }
}
