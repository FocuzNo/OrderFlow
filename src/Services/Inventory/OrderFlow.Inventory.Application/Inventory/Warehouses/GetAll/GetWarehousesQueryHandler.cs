using OrderFlow.Inventory.Application.Abstractions.Errors;
using OrderFlow.Inventory.Application.Abstractions.Messaging;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Stock;
using OrderFlow.Inventory.Domain.Warehouses;

namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class GetWarehousesQueryHandler(IInventoryRepository repository)
        : IRequestHandler<GetWarehousesQuery, IReadOnlyList<WarehouseResponse>>
    {
        public async Task<IReadOnlyList<WarehouseResponse>> Handle(
            GetWarehousesQuery q,
            CancellationToken cancellationToken
        ) =>
            (await repository.ListWarehousesAsync(cancellationToken))
                .Select(entity => new WarehouseResponse(entity.Id, entity.Name, entity.Location))
                .ToArray();
    }
}
