namespace OrderFlow.Inventory.Application.Inventory;

public sealed class ReserveInventoryCommandValidator
    : AbstractValidator<InventoryFeatures.ReserveInventoryCommand>
{
    public ReserveInventoryCommandValidator()
    {
        RuleFor(candidate => candidate.OrderId).NotEmpty();
        RuleFor(candidate => candidate.Quantity).GreaterThan(0);
    }
}
