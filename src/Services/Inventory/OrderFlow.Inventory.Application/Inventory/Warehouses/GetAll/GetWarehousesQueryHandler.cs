using MediatR;
using OrderFlow.Inventory.Application.Abstractions.Errors;
using OrderFlow.Inventory.Application.Abstractions.Messaging;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Stock;
using OrderFlow.Inventory.Domain.Warehouses;

namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class GetWarehousesQueryHandler(IInventoryRepository r)
        : IRequestHandler<GetWarehousesQuery, IReadOnlyList<WarehouseResponse>>
    {
        public async Task<IReadOnlyList<WarehouseResponse>> Handle(
            GetWarehousesQuery q,
            CancellationToken ct
        ) =>
            (await r.ListWarehousesAsync(ct))
                .Select(x => new WarehouseResponse(x.Id, x.Name, x.Location))
                .ToArray();
    }
}
