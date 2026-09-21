using FluentValidation;

namespace OrderFlow.Inventory.Application.Inventory;

public sealed class CreateWarehouseValidator : AbstractValidator<InventoryFeatures.CreateWarehouse>
{
    public CreateWarehouseValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(500);
    }
}

public sealed class CreateStockValidator : AbstractValidator<InventoryFeatures.CreateStock>
{
    public CreateStockValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(64);
    }
}

public sealed class IncreaseStockValidator : AbstractValidator<InventoryFeatures.Increase>
{
    public IncreaseStockValidator() => RuleFor(x => x.Quantity).GreaterThan(0);
}

public sealed class DecreaseStockValidator : AbstractValidator<InventoryFeatures.Decrease>
{
    public DecreaseStockValidator() => RuleFor(x => x.Quantity).GreaterThan(0);
}

public sealed class ReserveStockValidator : AbstractValidator<InventoryFeatures.Reserve>
{
    public ReserveStockValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
