namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class GetStockItemQueryValidator : AbstractValidator<GetStockItemQuery>
    {
        public GetStockItemQueryValidator()
        {
            RuleFor(query => query.ProductId).NotEmpty();
            RuleFor(query => query.WarehouseId).NotEmpty();
        }
    }
}
