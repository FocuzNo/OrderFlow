namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class GetInventoryQueryValidator : AbstractValidator<GetInventoryQuery>
    {
        public GetInventoryQueryValidator()
        {
            RuleFor(query => query.Page).GreaterThan(0);
            RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
        }
    }
}
