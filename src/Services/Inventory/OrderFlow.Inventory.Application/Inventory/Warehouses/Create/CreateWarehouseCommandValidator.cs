namespace OrderFlow.Inventory.Application.Inventory;

public sealed class CreateWarehouseCommandValidator
    : AbstractValidator<InventoryFeatures.CreateWarehouseCommand>
{
    public CreateWarehouseCommandValidator()
    {
        RuleFor(candidate => candidate.Name).NotEmpty().MaximumLength(120);
        RuleFor(candidate => candidate.Location).NotEmpty().MaximumLength(500);
    }
}
