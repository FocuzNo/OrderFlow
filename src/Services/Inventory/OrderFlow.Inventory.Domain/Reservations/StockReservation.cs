using OrderFlow.Inventory.Domain.Common;

namespace OrderFlow.Inventory.Domain.Reservations;

public sealed class StockReservation : Entity
{
    private StockReservation() { }

    private StockReservation(Guid id, Guid stockItemId, Guid orderId, int quantity)
        : base(id)
    {
        StockItemId = stockItemId;
        OrderId = orderId;
        Quantity = quantity;
        Status = ReservationStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid StockItemId { get; private set; }

    public Guid OrderId { get; private set; }

    public int Quantity { get; private set; }

    public ReservationStatus Status { get; private set; } = ReservationStatus.Pending;

    public DateTimeOffset CreatedAt { get; private set; }

    public static StockReservation Create(Guid stockItemId, Guid orderId, int quantity) =>
        new(Guid.NewGuid(), stockItemId, orderId, quantity);

    public void Confirm()
    {
        EnsurePending();
        Status = ReservationStatus.Confirmed;
    }

    public void Release()
    {
        EnsurePending();
        Status = ReservationStatus.Released;
    }

    private void EnsurePending()
    {
        if (Status != ReservationStatus.Pending)
            throw new DomainException("Only pending reservations can change state.");
    }
}
