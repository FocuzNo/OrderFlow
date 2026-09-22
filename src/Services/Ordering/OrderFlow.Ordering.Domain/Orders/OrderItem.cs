using OrderFlow.Ordering.Domain.Common;

namespace OrderFlow.Ordering.Domain.Orders;

public sealed class OrderItem : Entity
{
    private OrderItem() { }

    private OrderItem(Guid id, Guid productId, string productName, decimal unitPrice, int quantity)
        : base(id)
    {
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public Guid ProductId { get; private set; }

    public string ProductName { get; private set; } = string.Empty;

    public decimal UnitPrice { get; private set; }

    public int Quantity { get; private set; }

    public decimal Total => UnitPrice * Quantity;

    public static OrderItem Create(
        Guid productId,
        string productName,
        decimal unitPrice,
        int quantity
    )
    {
        if (
            productId == Guid.Empty
            || string.IsNullOrWhiteSpace(productName)
            || productName.Length > 200
        )
            throw new DomainException("Product snapshot is required.");
        if (unitPrice < 0 || quantity <= 0)
            throw new DomainException(
                "Unit price cannot be negative and quantity must be positive."
            );
        return new OrderItem(
            Guid.NewGuid(),
            productId,
            productName.Trim(),
            decimal.Round(unitPrice, 2),
            quantity
        );
    }

    public void Increase(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be positive.");
        checked
        {
            Quantity += quantity;
        }
    }
}
