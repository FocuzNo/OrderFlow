namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class CreateInventoryCommandValidator : AbstractValidator<CreateInventoryCommand>
    {
        public CreateInventoryCommandValidator()
        {
            RuleFor(command => command.ProductId).NotEmpty();
            RuleFor(command => command.WarehouseId).NotEmpty();
            RuleFor(command => command.Sku).NotEmpty().MaximumLength(64);
            RuleFor(command => command.Quantity).GreaterThanOrEqualTo(0);
        }
    }
}
