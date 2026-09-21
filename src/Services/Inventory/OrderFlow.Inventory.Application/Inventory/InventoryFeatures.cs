using MediatR;
using OrderFlow.Inventory.Application.Abstractions.Errors;
using OrderFlow.Inventory.Application.Abstractions.Messaging;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Stock;
using OrderFlow.Inventory.Domain.Warehouses;

namespace OrderFlow.Inventory.Application.Inventory;

public static class InventoryFeatures
{
    public sealed record WarehouseDto(Guid Id, string Name, string Location);

    public sealed record StockDto(
        Guid Id,
        Guid ProductId,
        Guid WarehouseId,
        string Sku,
        int OnHand,
        int Reserved,
        int Available
    );

    public sealed record ReservationDto(
        Guid Id,
        Guid OrderId,
        Guid StockItemId,
        int Quantity,
        string Status
    );

    public sealed record CreateWarehouse(string Name, string Location) : ICommand<WarehouseDto>;

    public sealed class CreateWarehouseHandler(IInventoryRepository r)
        : IRequestHandler<CreateWarehouse, WarehouseDto>
    {
        public async Task<WarehouseDto> Handle(CreateWarehouse c, CancellationToken ct)
        {
            var x = Warehouse.Create(c.Name, c.Location);
            await r.AddWarehouseAsync(x, ct);
            return new(x.Id, x.Name, x.Location);
        }
    }

    public sealed record GetWarehouses : IQuery<IReadOnlyList<WarehouseDto>>;

    public sealed class GetWarehousesHandler(IInventoryRepository r)
        : IRequestHandler<GetWarehouses, IReadOnlyList<WarehouseDto>>
    {
        public async Task<IReadOnlyList<WarehouseDto>> Handle(
            GetWarehouses q,
            CancellationToken ct
        ) =>
            (await r.ListWarehousesAsync(ct))
                .Select(x => new WarehouseDto(x.Id, x.Name, x.Location))
                .ToArray();
    }

    public sealed record CreateStock(Guid ProductId, Guid WarehouseId, string Sku)
        : ICommand<StockDto>;

    public sealed class CreateStockHandler(IInventoryRepository r)
        : IRequestHandler<CreateStock, StockDto>
    {
        public async Task<StockDto> Handle(CreateStock c, CancellationToken ct)
        {
            if (await r.GetStockAsync(c.ProductId, c.WarehouseId, ct) is not null)
                throw new ConflictException("Stock item already exists.");
            var x = StockItem.Create(c.ProductId, c.WarehouseId, c.Sku);
            await r.AddStockItemAsync(x, ct);
            return Map(x);
        }
    }

    public sealed record GetStock(Guid ProductId, Guid WarehouseId) : IQuery<StockDto>;

    public sealed class GetStockHandler(IInventoryRepository r)
        : IRequestHandler<GetStock, StockDto>
    {
        public async Task<StockDto> Handle(GetStock q, CancellationToken ct) =>
            Map(await Find(r, q.ProductId, q.WarehouseId, ct));
    }

    public sealed record Increase(Guid ProductId, Guid WarehouseId, int Quantity)
        : ICommand<StockDto>;

    public sealed class IncreaseHandler(IInventoryRepository r)
        : IRequestHandler<Increase, StockDto>
    {
        public async Task<StockDto> Handle(Increase c, CancellationToken ct)
        {
            var x = await Find(r, c.ProductId, c.WarehouseId, ct);
            x.Increase(c.Quantity);
            await r.SaveAsync(ct);
            return Map(x);
        }
    }

    public sealed record Decrease(Guid ProductId, Guid WarehouseId, int Quantity)
        : ICommand<StockDto>;

    public sealed class DecreaseHandler(IInventoryRepository r)
        : IRequestHandler<Decrease, StockDto>
    {
        public async Task<StockDto> Handle(Decrease c, CancellationToken ct)
        {
            var x = await Find(r, c.ProductId, c.WarehouseId, ct);
            x.Decrease(c.Quantity);
            await r.SaveAsync(ct);
            return Map(x);
        }
    }

    public sealed record Reserve(Guid ProductId, Guid WarehouseId, Guid OrderId, int Quantity)
        : ICommand<ReservationDto>;

    public sealed class ReserveHandler(IInventoryRepository r)
        : IRequestHandler<Reserve, ReservationDto>
    {
        public async Task<ReservationDto> Handle(Reserve c, CancellationToken ct)
        {
            var x = await Find(r, c.ProductId, c.WarehouseId, ct);
            var z = x.Reserve(c.OrderId, c.Quantity);
            await r.SaveAsync(ct);
            return Map(z);
        }
    }

    public sealed record Confirm(Guid ProductId, Guid WarehouseId, Guid ReservationId) : ICommand;

    public sealed class ConfirmHandler(IInventoryRepository r) : IRequestHandler<Confirm>
    {
        public async Task Handle(Confirm c, CancellationToken ct)
        {
            var x = await Find(r, c.ProductId, c.WarehouseId, ct);
            x.Confirm(c.ReservationId);
            await r.SaveAsync(ct);
        }
    }

    public sealed record Release(Guid ProductId, Guid WarehouseId, Guid ReservationId) : ICommand;

    public sealed class ReleaseHandler(IInventoryRepository r) : IRequestHandler<Release>
    {
        public async Task Handle(Release c, CancellationToken ct)
        {
            var x = await Find(r, c.ProductId, c.WarehouseId, ct);
            x.Release(c.ReservationId);
            await r.SaveAsync(ct);
        }
    }

    public sealed record GetReservation(Guid Id) : IQuery<ReservationDto>;

    public sealed class GetReservationHandler(IInventoryRepository r)
        : IRequestHandler<GetReservation, ReservationDto>
    {
        public async Task<ReservationDto> Handle(GetReservation q, CancellationToken ct) =>
            Map(
                await r.GetReservationAsync(q.Id, ct)
                    ?? throw new NotFoundException("Reservation was not found.")
            );
    }

    private static async Task<StockItem> Find(
        IInventoryRepository r,
        Guid p,
        Guid w,
        CancellationToken ct
    ) =>
        await r.GetStockAsync(p, w, ct) ?? throw new NotFoundException("Stock item was not found.");

    private static StockDto Map(StockItem x) =>
        new(
            x.Id,
            x.ProductId,
            x.WarehouseId,
            x.Sku,
            x.QuantityOnHand,
            x.ReservedQuantity,
            x.AvailableQuantity
        );

    private static ReservationDto Map(Domain.Reservations.StockReservation x) =>
        new(x.Id, x.OrderId, x.StockItemId, x.Quantity, x.Status.Name);
}
