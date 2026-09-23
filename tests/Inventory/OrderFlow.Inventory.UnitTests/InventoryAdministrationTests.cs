using OrderFlow.Inventory.Application.Inventory;
using OrderFlow.Inventory.Domain.Common;
using OrderFlow.Inventory.Domain.Stock;

namespace OrderFlow.Inventory.UnitTests;

public sealed class InventoryAdministrationTests
{
    [Fact]
    public void UpdateQuantity_ShouldPreserveReservedStock()
    {
        var stock = StockItem.Create(Guid.NewGuid(), Guid.NewGuid(), "SKU");
        stock.UpdateQuantity(10);
        stock.Reserve(Guid.NewGuid(), 4);
        Assert.Throws<DomainException>(() => stock.UpdateQuantity(3));
        Assert.Equal(6, stock.AvailableQuantity);
        Assert.Throws<DomainException>(() => stock.EnsureCanDelete());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validator_ShouldRejectNegativeQuantity(int quantity) =>
        Assert.False(
            new InventoryFeatures.CreateInventoryCommandValidator()
                .Validate(
                    new InventoryFeatures.CreateInventoryCommand(
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        "SKU",
                        quantity
                    )
                )
                .IsValid
        );

    [Fact]
    public void Delete_ShouldAllowUnreservedStock()
    {
        var stock = StockItem.Create(Guid.NewGuid(), Guid.NewGuid(), "SKU");
        stock.UpdateQuantity(1);
        stock.EnsureCanDelete();
    }
}
