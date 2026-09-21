using OrderFlow.Ordering.Domain.Common;
using OrderFlow.Ordering.Domain.Orders;

namespace OrderFlow.Ordering.UnitTests;

public sealed class OrderTests
{
    private static Order CreateOrder() =>
        Order.Create(
            Guid.NewGuid(),
            "buyer@example.test",
            ShippingAddress.Create("1 Main St", "Minsk", "220000", "BY")
        );

    [Fact]
    public void Submit_moves_order_to_inventory_and_raises_event()
    {
        var order = CreateOrder();
        order.AddItem(Guid.NewGuid(), "Notebook", 12.50m, 2);
        order.Submit();

        Assert.Equal(OrderStatus.PendingInventory, order.Status);
        Assert.Equal(25m, order.TotalAmount);
        Assert.Contains(order.DomainEvents, x => x is OrderSubmittedDomainEvent);
    }

    [Fact]
    public void Submitted_order_cannot_be_edited()
    {
        var order = CreateOrder();
        order.AddItem(Guid.NewGuid(), "Notebook", 10m, 1);
        order.Submit();
        Assert.Throws<DomainException>(() => order.AddItem(Guid.NewGuid(), "Pen", 2m, 1));
    }
}
