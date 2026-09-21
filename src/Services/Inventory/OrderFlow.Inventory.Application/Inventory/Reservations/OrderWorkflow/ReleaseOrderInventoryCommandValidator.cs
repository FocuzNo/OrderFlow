namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class ReleaseOrderInventoryCommandValidator
        : AbstractValidator<ReleaseOrderInventoryCommand>
    {
        public ReleaseOrderInventoryCommandValidator() => RuleFor(x => x.OrderId).NotEmpty();
    }
}
