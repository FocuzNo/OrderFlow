namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class UpdateInventoryCommandValidator : AbstractValidator<UpdateInventoryCommand>
    {
        public UpdateInventoryCommandValidator()
        {
            RuleFor(command => command.ProductId).NotEmpty();
            RuleFor(command => command.Quantity).GreaterThanOrEqualTo(0);
        }
    }
}
