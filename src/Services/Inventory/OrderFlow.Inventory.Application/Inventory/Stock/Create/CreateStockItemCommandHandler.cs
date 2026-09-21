using MediatR;
using OrderFlow.Inventory.Application.Abstractions.Errors;
using OrderFlow.Inventory.Application.Abstractions.Messaging;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Stock;
using OrderFlow.Inventory.Domain.Warehouses;

namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class CreateStockItemCommandHandler(IInventoryRepository r)
        : IRequestHandler<CreateStockItemCommand, StockResponse>
    {
        public async Task<StockResponse> Handle(CreateStockItemCommand c, CancellationToken ct)
        {
            if (await r.GetStockAsync(c.ProductId, c.WarehouseId, ct) is not null)
                throw new ConflictException("Stock item already exists.");
            var x = StockItem.Create(c.ProductId, c.WarehouseId, c.Sku);
            await r.AddStockItemAsync(x, ct);
            return Map(x);
        }
    }
}
