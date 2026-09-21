using FastEndpoints;
using MediatR;
using F = OrderFlow.Inventory.Application.Inventory.InventoryFeatures;

namespace OrderFlow.Inventory.Api.Endpoints;

public static partial class InventoryEndpoints
{
    public sealed class CreateStockItemEndpoint(ISender s)
        : Endpoint<CreateStockItemRequest, F.StockResponse>
    {
        public override void Configure()
        {
            Post("/api/stock-items");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CreateStockItemRequest r, CancellationToken ct) =>
            await Send.ResponseAsync(
                await s.Send(new F.CreateStockItemCommand(r.ProductId, r.WarehouseId, r.Sku), ct),
                201,
                ct
            );
    }
}
