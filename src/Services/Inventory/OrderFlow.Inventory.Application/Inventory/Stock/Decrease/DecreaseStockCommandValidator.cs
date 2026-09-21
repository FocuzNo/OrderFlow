using FluentValidation;

namespace OrderFlow.Inventory.Application.Inventory;

public sealed class DecreaseStockCommandValidator
    : AbstractValidator<InventoryFeatures.DecreaseStockCommand>
{
    public DecreaseStockCommandValidator() => RuleFor(x => x.Quantity).GreaterThan(0);
}
