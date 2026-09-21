using OrderFlow.Ordering.Domain.Common;
namespace OrderFlow.Ordering.Domain.Orders;
public sealed class Order : AggregateRoot
{
    private readonly List<OrderItem> _items = [];
    private Order() { }
    private Order(Guid id, Guid customerId, EmailAddress email, ShippingAddress address, DateTimeOffset now) : base(id) { CustomerId = customerId; CustomerEmail = email; ShippingAddress = address; Status = OrderStatus.Draft; CreatedAt = now; UpdatedAt = now; }
    public Guid CustomerId { get; private set; } public EmailAddress CustomerEmail { get; private set; } public ShippingAddress ShippingAddress { get; private set; } = null!; public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly(); public decimal TotalAmount => _items.Sum(x => x.Total); public OrderStatus Status { get; private set; } = OrderStatus.Draft; public DateTimeOffset CreatedAt { get; private set; } public DateTimeOffset UpdatedAt { get; private set; } public uint Version { get; private set; }
    public static Order Create(Guid customerId, string email, ShippingAddress address) { if (customerId == Guid.Empty) throw new DomainException("Customer is required."); return new Order(Guid.NewGuid(), customerId, EmailAddress.Create(email), address, DateTimeOffset.UtcNow); }
    public void AddItem(Guid productId, string productName, decimal unitPrice, int quantity) { EnsureDraft(); var existing = _items.SingleOrDefault(x => x.ProductId == productId); if (existing is null) _items.Add(OrderItem.Create(productId, productName, unitPrice, quantity)); else { if (existing.UnitPrice != decimal.Round(unitPrice, 2)) throw new DomainException("Product snapshot price does not match the existing item."); existing.Increase(quantity); } Touch(); }
    public void RemoveItem(Guid itemId) { EnsureDraft(); var item = _items.SingleOrDefault(x => x.Id == itemId) ?? throw new DomainException("Order item was not found."); _items.Remove(item); Touch(); }
    public void Submit() { EnsureDraft(); if (_items.Count == 0) throw new DomainException("An order must contain at least one item."); Status = OrderStatus.PendingInventory; Touch(); Raise(new OrderSubmittedDomainEvent(Guid.NewGuid(), UpdatedAt, Id, CustomerId, CustomerEmail.Value, _items.Select(x => new OrderItemSnapshot(x.ProductId, x.ProductName, x.UnitPrice, x.Quantity)).ToArray(), TotalAmount)); }
    public void MarkInventoryReserved() { if (Status != OrderStatus.PendingInventory) throw new DomainException("Order is not awaiting inventory."); Status = OrderStatus.PendingPayment; Touch(); Raise(new PaymentRequestedDomainEvent(Guid.NewGuid(), UpdatedAt, Id, TotalAmount)); }
    public void ConfirmPayment() { if (Status != OrderStatus.PendingPayment) throw new DomainException("Order is not awaiting payment."); Status = OrderStatus.Confirmed; Touch(); Raise(new OrderConfirmedDomainEvent(Guid.NewGuid(), UpdatedAt, Id, CustomerId, CustomerEmail.Value)); }
    public void Cancel(string reason) { if (Status == OrderStatus.Confirmed || Status == OrderStatus.Cancelled) throw new DomainException("Order cannot be cancelled in its current state."); if (string.IsNullOrWhiteSpace(reason)) throw new DomainException("Cancellation reason is required."); Status = OrderStatus.Cancelled; Touch(); Raise(new OrderCancelledDomainEvent(Guid.NewGuid(), UpdatedAt, Id, CustomerEmail.Value, reason.Trim())); }
    private void EnsureDraft() { if (Status != OrderStatus.Draft) throw new DomainException("Only draft orders can be edited."); }
    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
