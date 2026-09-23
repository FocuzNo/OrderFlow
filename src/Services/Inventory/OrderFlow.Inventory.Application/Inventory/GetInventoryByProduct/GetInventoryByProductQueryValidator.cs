namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class GetInventoryByProductQueryValidator
        : AbstractValidator<GetInventoryByProductQuery>
    {
        public GetInventoryByProductQueryValidator()
        {
            RuleFor(query => query.ProductId).NotEmpty();
        }
    }
}
