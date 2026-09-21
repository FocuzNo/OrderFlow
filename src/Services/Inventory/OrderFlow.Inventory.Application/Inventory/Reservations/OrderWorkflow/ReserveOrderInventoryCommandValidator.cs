namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class ReserveOrderInventoryCommandValidator
        : AbstractValidator<ReserveOrderInventoryCommand>
    {
        public ReserveOrderInventoryCommandValidator()
        {
            RuleFor(candidate => candidate.OrderId).NotEmpty();
            RuleFor(candidate => candidate.Items).NotEmpty();
            RuleForEach(candidate => candidate.Items)
                .ChildRules(item =>
                {
                    item.RuleFor(candidate => candidate.ProductId).NotEmpty();
                    item.RuleFor(candidate => candidate.Quantity).GreaterThan(0);
                });
        }
    }
}
