using MediatR;
using OrderFlow.Inventory.Application.Abstractions.Errors;
using OrderFlow.Inventory.Application.Abstractions.Messaging;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Stock;
using OrderFlow.Inventory.Domain.Warehouses;

namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class GetStockItemQueryHandler(IInventoryRepository r)
        : IRequestHandler<GetStockItemQuery, StockResponse>
    {
        public async Task<StockResponse> Handle(GetStockItemQuery q, CancellationToken ct) =>
            Map(await Find(r, q.ProductId, q.WarehouseId, ct));
    }
}
