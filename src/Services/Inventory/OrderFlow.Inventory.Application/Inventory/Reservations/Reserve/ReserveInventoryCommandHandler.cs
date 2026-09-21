using MediatR;
using OrderFlow.Inventory.Application.Abstractions.Errors;
using OrderFlow.Inventory.Application.Abstractions.Messaging;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Stock;
using OrderFlow.Inventory.Domain.Warehouses;

namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class ReserveInventoryCommandHandler(IInventoryRepository r)
        : IRequestHandler<ReserveInventoryCommand, ReservationResponse>
    {
        public async Task<ReservationResponse> Handle(
            ReserveInventoryCommand c,
            CancellationToken ct
        )
        {
            var x = await Find(r, c.ProductId, c.WarehouseId, ct);
            var z = x.Reserve(c.OrderId, c.Quantity);
            await r.SaveAsync(ct);
            return Map(z);
        }
    }
}
