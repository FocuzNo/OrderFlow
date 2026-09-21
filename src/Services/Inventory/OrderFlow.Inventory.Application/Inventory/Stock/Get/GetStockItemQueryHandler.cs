using OrderFlow.Inventory.Application.Abstractions.Errors;
using OrderFlow.Inventory.Application.Abstractions.Messaging;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Stock;
using OrderFlow.Inventory.Domain.Warehouses;

namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class GetStockItemQueryHandler(IInventoryRepository repository)
        : IRequestHandler<GetStockItemQuery, StockResponse>
    {
        public async Task<StockResponse> Handle(
            GetStockItemQuery q,
            CancellationToken cancellationToken
        ) => Map(await Find(repository, q.ProductId, q.WarehouseId, cancellationToken));
    }
}
