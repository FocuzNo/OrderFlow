using FluentValidation;

namespace OrderFlow.Inventory.Application.Inventory;

public sealed class CreateWarehouseCommandValidator
    : AbstractValidator<InventoryFeatures.CreateWarehouseCommand>
{
    public CreateWarehouseCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(500);
    }
}
