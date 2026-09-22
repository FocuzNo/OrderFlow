using OrderFlow.Inventory.Domain.Common;
using OrderFlow.Inventory.Domain.Reservations;

namespace OrderFlow.Inventory.Domain.Stock;

public sealed class StockItem : AggregateRoot
{
    private readonly List<StockReservation> _reservations = [];

    private StockItem() { }

    private StockItem(Guid id, Guid productId, Guid warehouseId, string sku)
        : base(id)
    {
        ProductId = productId;
        WarehouseId = warehouseId;
        Sku = sku;
    }

    public Guid ProductId { get; private set; }

    public Guid WarehouseId { get; private set; }

    public string Sku { get; private set; } = string.Empty;

    public int QuantityOnHand { get; private set; }

    public int ReservedQuantity { get; private set; }

    public int AvailableQuantity => QuantityOnHand - ReservedQuantity;

    public uint Version { get; private set; }

    public IReadOnlyCollection<StockReservation> Reservations => _reservations.AsReadOnly();

    public static StockItem Create(Guid productId, Guid warehouseId, string sku)
    {
        if (productId == Guid.Empty || warehouseId == Guid.Empty)
            throw new DomainException("Product and warehouse are required.");
        if (string.IsNullOrWhiteSpace(sku) || sku.Length > 64)
            throw new DomainException("SKU must contain 1-64 characters.");
        return new StockItem(Guid.NewGuid(), productId, warehouseId, sku.Trim().ToUpperInvariant());
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity < ReservedQuantity)
            throw new DomainException("Quantity cannot be below reserved quantity.");
        QuantityOnHand = quantity;
    }

    public void EnsureCanDelete()
    {
        if (ReservedQuantity != 0)
            throw new DomainException("Reserved inventory cannot be deleted.");
    }

    public void Increase(int quantity)
    {
        EnsurePositive(quantity);
        checked
        {
            QuantityOnHand += quantity;
        }
    }

    public void Decrease(int quantity)
    {
        EnsurePositive(quantity);
        if (quantity > AvailableQuantity)
            throw new DomainException("Insufficient available stock.");
        QuantityOnHand -= quantity;
    }

    public StockReservation Reserve(Guid orderId, int quantity)
    {
        EnsurePositive(quantity);
        if (orderId == Guid.Empty)
            throw new DomainException("Order is required.");
        if (
            _reservations.Any(candidate =>
                candidate.OrderId == orderId && candidate.Status == ReservationStatus.Pending
            )
        )
            throw new DomainException(
                "Order already has an active reservation for this stock item."
            );
        if (quantity > AvailableQuantity)
            throw new DomainException("Insufficient available stock.");
        var reservation = StockReservation.Create(Id, orderId, quantity);
        _reservations.Add(reservation);
        ReservedQuantity += quantity;

        return reservation;
    }

    public void Confirm(Guid reservationId)
    {
        var reservation = Find(reservationId);
        reservation.Confirm();
        ReservedQuantity -= reservation.Quantity;
        QuantityOnHand -= reservation.Quantity;
    }

    public void Release(Guid reservationId)
    {
        var reservation = Find(reservationId);
        reservation.Release();
        ReservedQuantity -= reservation.Quantity;
    }

    private StockReservation Find(Guid id) =>
        _reservations.SingleOrDefault(candidate => candidate.Id == id)
        ?? throw new DomainException("Reservation was not found on this stock item.");

    private static void EnsurePositive(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be positive.");
    }
}
