using OrderFlow.Ordering.Domain.Common;
using OrderFlow.Ordering.Domain.Orders;

namespace OrderFlow.Ordering.UnitTests;

public sealed class OrderTests
{
    private static Order CreateOrder() =>
        Order.Create(
            Guid.NewGuid(),
            "buyer@example.test",
            ShippingAddress.Create("Main", "Minsk", "220000", "BY"),
            [OrderItem.Create(Guid.NewGuid(), "Notebook", 12.5m, 2)]
        );

    [Fact]
    public void Create_ShouldCalculateTotalFromItems()
    {
        var order = CreateOrder();
        Assert.Equal(25m, order.TotalAmount);
        Assert.Equal(OrderStatus.PendingInventory, order.Status);
    }

    [Fact]
    public void Confirm_ShouldRequireInventoryAndPaymentProcessing()
    {
        var order = CreateOrder();
        Assert.Throws<DomainException>(() => order.Confirm());
        Assert.Throws<DomainException>(() => order.MarkPaymentProcessing());
        order.MarkInventoryReserved();
        Assert.Throws<DomainException>(() => order.Confirm());
        order.MarkPaymentProcessing();
        order.Confirm();
        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public void Create_ShouldRejectEmptyOrder() =>
        Assert.Throws<DomainException>(() =>
            Order.Create(
                Guid.NewGuid(),
                "buyer@example.test",
                ShippingAddress.Create("Main", "Minsk", "220000", "BY"),
                []
            )
        );

    [Fact]
    public void Cancel_ShouldRejectConfirmedOrder()
    {
        var order = CreateOrder();
        order.MarkInventoryReserved();
        order.MarkPaymentProcessing();
        order.Confirm();
        Assert.Throws<DomainException>(() => order.Cancel("Cancelled"));
    }
}
