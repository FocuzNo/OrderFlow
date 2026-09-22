namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class DeleteInventoryCommandValidator : AbstractValidator<DeleteInventoryCommand>
    {
        public DeleteInventoryCommandValidator()
        {
            RuleFor(command => command.ProductId).NotEmpty();
        }
    }
}
