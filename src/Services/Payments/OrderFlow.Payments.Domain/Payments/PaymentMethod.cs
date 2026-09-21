using Ardalis.SmartEnum;

namespace OrderFlow.Payments.Domain.Payments;

public sealed class PaymentMethod : SmartEnum<PaymentMethod>
{
    public static readonly PaymentMethod Card = new(nameof(Card), 1);
    public static readonly PaymentMethod BankTransfer = new(nameof(BankTransfer), 2);

    private PaymentMethod(string name, int value)
        : base(name, value) { }
}
