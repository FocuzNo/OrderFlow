using OrderFlow.Inventory.Application.Abstractions.Persistence;

namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class GetInventoryQueryHandler(IInventoryRepository repository)
        : IRequestHandler<GetInventoryQuery, IReadOnlyList<StockResponse>>
    {
        public async Task<IReadOnlyList<StockResponse>> Handle(
            GetInventoryQuery query,
            CancellationToken cancellationToken
        ) =>
            (await repository.ListAsync(query.Page, query.PageSize, cancellationToken))
                .Select(Map)
                .ToArray();
    }
}
