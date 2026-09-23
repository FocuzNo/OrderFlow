using OrderFlow.Inventory.Application.Abstractions.Errors;
using OrderFlow.Inventory.Application.Abstractions.Persistence;

namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class GetInventoryByProductQueryHandler(IInventoryRepository repository)
        : IRequestHandler<GetInventoryByProductQuery, StockResponse>
    {
        public async Task<StockResponse> Handle(
            GetInventoryByProductQuery query,
            CancellationToken cancellationToken
        ) =>
            Map(
                await repository.GetByProductIdAsync(query.ProductId, cancellationToken)
                    ?? throw new NotFoundException("Inventory was not found.")
            );
    }
}
