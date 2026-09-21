using Ardalis.SmartEnum;

namespace OrderFlow.Inventory.Domain.Reservations;

public sealed class ReservationStatus : SmartEnum<ReservationStatus>
{
    public static readonly ReservationStatus Pending = new(nameof(Pending), 0);
    public static readonly ReservationStatus Confirmed = new(nameof(Confirmed), 1);
    public static readonly ReservationStatus Released = new(nameof(Released), 2);

    private ReservationStatus(string name, int value)
        : base(name, value) { }
}
