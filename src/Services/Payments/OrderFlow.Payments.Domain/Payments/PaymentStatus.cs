using Ardalis.SmartEnum;

namespace OrderFlow.Payments.Domain.Payments;

public sealed class PaymentStatus : SmartEnum<PaymentStatus>
{
    public static readonly PaymentStatus Pending = new(nameof(Pending), 0);
    public static readonly PaymentStatus Processing = new(nameof(Processing), 1);
    public static readonly PaymentStatus Succeeded = new(nameof(Succeeded), 2);
    public static readonly PaymentStatus Failed = new(nameof(Failed), 3);
    public static readonly PaymentStatus Refunded = new(nameof(Refunded), 4);

    private PaymentStatus(string name, int value)
        : base(name, value) { }
}
