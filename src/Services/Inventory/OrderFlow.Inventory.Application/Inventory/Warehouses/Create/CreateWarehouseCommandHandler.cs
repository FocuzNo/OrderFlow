using MediatR;
using OrderFlow.Inventory.Application.Abstractions.Errors;
using OrderFlow.Inventory.Application.Abstractions.Messaging;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Stock;
using OrderFlow.Inventory.Domain.Warehouses;

namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class CreateWarehouseCommandHandler(IInventoryRepository r)
        : IRequestHandler<CreateWarehouseCommand, WarehouseResponse>
    {
        public async Task<WarehouseResponse> Handle(CreateWarehouseCommand c, CancellationToken ct)
        {
            var x = Warehouse.Create(c.Name, c.Location);
            await r.AddWarehouseAsync(x, ct);
            return new(x.Id, x.Name, x.Location);
        }
    }
}
