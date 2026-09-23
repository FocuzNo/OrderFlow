namespace OrderFlow.Inventory.Application.Inventory;

public sealed class CreateStockItemCommandValidator
    : AbstractValidator<InventoryFeatures.CreateStockItemCommand>
{
    public CreateStockItemCommandValidator()
    {
        RuleFor(candidate => candidate.ProductId).NotEmpty();
        RuleFor(candidate => candidate.WarehouseId).NotEmpty();
        RuleFor(candidate => candidate.Sku).NotEmpty().MaximumLength(64);
    }
}
