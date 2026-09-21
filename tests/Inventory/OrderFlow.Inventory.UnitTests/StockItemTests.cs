using OrderFlow.Inventory.Domain.Common;
using OrderFlow.Inventory.Domain.Stock;

namespace OrderFlow.Inventory.UnitTests;

public sealed class StockItemTests
{
    [Fact]
    public void Reserve_and_confirm_updates_quantities()
    {
        var stock = StockItem.Create(Guid.NewGuid(), Guid.NewGuid(), "sku-1");
        stock.Increase(10);
        var reservation = stock.Reserve(Guid.NewGuid(), 4);

        Assert.Equal(6, stock.AvailableQuantity);
        Assert.Equal(4, stock.ReservedQuantity);
        stock.Confirm(reservation.Id);
        Assert.Equal(6, stock.QuantityOnHand);
        Assert.Equal(0, stock.ReservedQuantity);
    }

    [Fact]
    public void Reserve_rejects_insufficient_stock()
    {
        var stock = StockItem.Create(Guid.NewGuid(), Guid.NewGuid(), "SKU-1");
        stock.Increase(2);
        Assert.Throws<DomainException>(() => stock.Reserve(Guid.NewGuid(), 3));
    }
}
