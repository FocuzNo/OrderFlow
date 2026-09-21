using FluentValidation;

namespace OrderFlow.Inventory.Application.Inventory;

public sealed class CreateStockItemCommandValidator
    : AbstractValidator<InventoryFeatures.CreateStockItemCommand>
{
    public CreateStockItemCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(64);
    }
}
