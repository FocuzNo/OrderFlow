namespace OrderFlow.Inventory.Application.Inventory;

public sealed class DecreaseStockCommandValidator
    : AbstractValidator<InventoryFeatures.DecreaseStockCommand>
{
    public DecreaseStockCommandValidator() =>
        RuleFor(candidate => candidate.Quantity).GreaterThan(0);
}
