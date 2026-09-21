using FastEndpoints;
using MediatR;
using F = OrderFlow.Inventory.Application.Inventory.InventoryFeatures;

namespace OrderFlow.Inventory.Api.Endpoints;

public static partial class InventoryEndpoints
{
    public sealed class IncreaseStockEndpoint(ISender s)
        : Endpoint<AdjustStockRequest, F.StockResponse>
    {
        public override void Configure()
        {
            Post("/api/stock-items/increase");
            AllowAnonymous();
        }

        public override async Task HandleAsync(AdjustStockRequest r, CancellationToken ct) =>
            await Send.OkAsync(
                await s.Send(
                    new F.IncreaseStockCommand(r.ProductId, r.WarehouseId, r.Quantity),
                    ct
                ),
                ct
            );
    }
}
