using Ardalis.SmartEnum;

namespace OrderFlow.Ordering.Domain.Orders;

public sealed class OrderStatus : SmartEnum<OrderStatus>
{
    public static readonly OrderStatus Draft = new(nameof(Draft), 0);
    public static readonly OrderStatus PendingInventory = new(nameof(PendingInventory), 1);
    public static readonly OrderStatus PendingPayment = new(nameof(PendingPayment), 2);
    public static readonly OrderStatus Confirmed = new(nameof(Confirmed), 3);
    public static readonly OrderStatus Cancelled = new(nameof(Cancelled), 4);
    public static readonly OrderStatus InventoryReserved = new(nameof(InventoryReserved), 5);

    private OrderStatus(string name, int value)
        : base(name, value) { }
}
