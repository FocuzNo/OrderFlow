using MediatR;
using OrderFlow.Inventory.Application.Abstractions.Errors;
using OrderFlow.Inventory.Application.Abstractions.Messaging;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Stock;
using OrderFlow.Inventory.Domain.Warehouses;

namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class DecreaseStockCommandHandler(IInventoryRepository r)
        : IRequestHandler<DecreaseStockCommand, StockResponse>
    {
        public async Task<StockResponse> Handle(DecreaseStockCommand c, CancellationToken ct)
        {
            var x = await Find(r, c.ProductId, c.WarehouseId, ct);
            x.Decrease(c.Quantity);
            await r.SaveAsync(ct);
            return Map(x);
        }
    }
}
