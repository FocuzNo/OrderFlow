using FastEndpoints;
using MediatR;
using F = OrderFlow.Inventory.Application.Inventory.InventoryFeatures;

namespace OrderFlow.Inventory.Api.Endpoints;

public static partial class InventoryEndpoints
{
    public sealed class DecreaseStockEndpoint(ISender s)
        : Endpoint<AdjustStockRequest, F.StockResponse>
    {
        public override void Configure()
        {
            Post("/api/stock-items/decrease");
            AllowAnonymous();
        }

        public override async Task HandleAsync(AdjustStockRequest r, CancellationToken ct) =>
            await Send.OkAsync(
                await s.Send(
                    new F.DecreaseStockCommand(r.ProductId, r.WarehouseId, r.Quantity),
                    ct
                ),
                ct
            );
    }
}
