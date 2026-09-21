namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class ReserveOrderInventoryCommandValidator
        : AbstractValidator<ReserveOrderInventoryCommand>
    {
        public ReserveOrderInventoryCommandValidator()
        {
            RuleFor(x => x.OrderId).NotEmpty();
            RuleFor(x => x.Items).NotEmpty();
            RuleForEach(x => x.Items)
                .ChildRules(item =>
                {
                    item.RuleFor(x => x.ProductId).NotEmpty();
                    item.RuleFor(x => x.Quantity).GreaterThan(0);
                });
        }
    }
}
