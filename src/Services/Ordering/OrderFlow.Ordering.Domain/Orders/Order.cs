using OrderFlow.Ordering.Domain.Common;

namespace OrderFlow.Ordering.Domain.Orders;

public sealed class Order : AggregateRoot
{
    private readonly List<OrderItem> _items = [];

    private Order() { }

    private Order(
        Guid id,
        Guid customerId,
        EmailAddress email,
        ShippingAddress address,
        DateTimeOffset now
    )
        : base(id)
    {
        CustomerId = customerId;
        CustomerEmail = email;
        ShippingAddress = address;
        Status = OrderStatus.Draft;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public Guid CustomerId { get; private set; }

    public EmailAddress CustomerEmail { get; private set; }

    public ShippingAddress ShippingAddress { get; private set; } = null!;

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public decimal TotalAmount => _items.Sum(candidate => candidate.Total);

    public OrderStatus Status { get; private set; } = OrderStatus.Draft;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public uint Version { get; private set; }

    public static Order Create(
        Guid customerId,
        string email,
        ShippingAddress address,
        IReadOnlyList<OrderItem> items
    )
    {
        if (customerId == Guid.Empty)
            throw new DomainException("Customer is required.");
        if (address is null || items is null || items.Count == 0 || items.Any(item => item is null))
            throw new DomainException("Shipping address and at least one order item are required.");
        var order = new Order(
            Guid.NewGuid(),
            customerId,
            EmailAddress.Create(email),
            address,
            DateTimeOffset.UtcNow
        );
        foreach (var item in items)
            order.AddItem(item.ProductId, item.ProductName, item.UnitPrice, item.Quantity);
        order.Submit();
        return order;
    }

    public void AddItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        EnsureDraft();
        var existing = _items.SingleOrDefault(candidate => candidate.ProductId == productId);
        if (existing is null)
            _items.Add(OrderItem.Create(productId, productName, unitPrice, quantity));
        else
        {
            if (existing.UnitPrice != decimal.Round(unitPrice, 2))
                throw new DomainException(
                    "Product snapshot price does not match the existing item."
                );
            existing.Increase(quantity);
        }
        Touch();
    }

    public void RemoveItem(Guid itemId)
    {
        EnsureDraft();
        var item =
            _items.SingleOrDefault(candidate => candidate.Id == itemId)
            ?? throw new DomainException("Order item was not found.");
        _items.Remove(item);
        Touch();
    }

    public void Submit()
    {
        EnsureDraft();
        if (_items.Count == 0)
            throw new DomainException("An order must contain at least one item.");
        Status = OrderStatus.PendingInventory;
        Touch();
    }

    public void MarkInventoryReserved()
    {
        if (Status != OrderStatus.PendingInventory)
            throw new DomainException("Order is not awaiting inventory.");
        Status = OrderStatus.InventoryReserved;
        Touch();
    }

    public void MarkPaymentProcessing()
    {
        if (Status != OrderStatus.InventoryReserved)
            throw new DomainException("Inventory must be reserved before processing payment.");
        Status = OrderStatus.PendingPayment;
        Touch();
    }

    public void Confirm()
    {
        if (Status != OrderStatus.PendingPayment)
            throw new DomainException("Order is not awaiting payment.");
        Status = OrderStatus.Confirmed;
        Touch();
    }

    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Confirmed || Status == OrderStatus.Cancelled)
            throw new DomainException("Order cannot be cancelled in its current state.");
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Cancellation reason is required.");
        Status = OrderStatus.Cancelled;
        Touch();
    }

    private void EnsureDraft()
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("Only draft orders can be edited.");
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
