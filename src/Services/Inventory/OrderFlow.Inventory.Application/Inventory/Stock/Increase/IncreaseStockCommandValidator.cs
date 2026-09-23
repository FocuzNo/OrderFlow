namespace OrderFlow.Inventory.Application.Inventory;

public sealed class IncreaseStockCommandValidator
    : AbstractValidator<InventoryFeatures.IncreaseStockCommand>
{
    public IncreaseStockCommandValidator() =>
        RuleFor(candidate => candidate.Quantity).GreaterThan(0);
}
