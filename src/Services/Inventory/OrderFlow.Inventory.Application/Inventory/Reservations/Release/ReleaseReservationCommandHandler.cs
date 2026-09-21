using MediatR;
using OrderFlow.Inventory.Application.Abstractions.Errors;
using OrderFlow.Inventory.Application.Abstractions.Messaging;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Stock;
using OrderFlow.Inventory.Domain.Warehouses;

namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class ReleaseReservationCommandHandler(IInventoryRepository r)
        : IRequestHandler<ReleaseReservationCommand>
    {
        public async Task Handle(ReleaseReservationCommand c, CancellationToken ct)
        {
            var x = await Find(r, c.ProductId, c.WarehouseId, ct);
            x.Release(c.ReservationId);
            await r.SaveAsync(ct);
        }
    }
}
