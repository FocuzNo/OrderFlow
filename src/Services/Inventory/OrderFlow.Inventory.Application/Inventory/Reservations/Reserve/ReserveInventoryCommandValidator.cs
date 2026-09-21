using FluentValidation;

namespace OrderFlow.Inventory.Application.Inventory;

public sealed class ReserveInventoryCommandValidator
    : AbstractValidator<InventoryFeatures.ReserveInventoryCommand>
{
    public ReserveInventoryCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
