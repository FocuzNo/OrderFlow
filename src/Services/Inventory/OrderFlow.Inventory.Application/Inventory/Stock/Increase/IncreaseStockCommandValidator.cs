using FluentValidation;

namespace OrderFlow.Inventory.Application.Inventory;

public sealed class IncreaseStockCommandValidator
    : AbstractValidator<InventoryFeatures.IncreaseStockCommand>
{
    public IncreaseStockCommandValidator() => RuleFor(x => x.Quantity).GreaterThan(0);
}
