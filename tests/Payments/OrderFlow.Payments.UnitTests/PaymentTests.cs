using OrderFlow.Payments.Domain.Common;
using OrderFlow.Payments.Domain.Payments;

namespace OrderFlow.Payments.UnitTests;

public sealed class PaymentTests
{
    [Fact]
    public void Successful_payment_can_be_fully_refunded()
    {
        var payment = Payment.Create(Guid.NewGuid(), 99m, PaymentMethod.Card);
        payment.StartProcessing();
        payment.Succeed("provider-1");
        payment.Refund(99m, "Customer request");

        Assert.Equal(PaymentStatus.Refunded, payment.Status);
        Assert.Single(payment.Refunds);
    }

    [Fact]
    public void Payment_requires_positive_amount() =>
        Assert.Throws<DomainException>(() => Payment.Create(Guid.NewGuid(), 0, PaymentMethod.Card));
}
