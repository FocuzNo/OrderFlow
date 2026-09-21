using OrderFlow.Payments.Domain.Common;
namespace OrderFlow.Payments.Domain.Payments;
public sealed class Payment : AggregateRoot
{
    private readonly List<Refund> _refunds = [];
    private Payment() { }
    private Payment(Guid id, Guid orderId, decimal amount, PaymentMethod method) : base(id) { OrderId = orderId; Amount = amount; Method = method; Status = PaymentStatus.Pending; CreatedAt = DateTimeOffset.UtcNow; UpdatedAt = CreatedAt; }
    public Guid OrderId { get; private set; } public decimal Amount { get; private set; } public PaymentMethod Method { get; private set; } = PaymentMethod.Card; public PaymentStatus Status { get; private set; } = PaymentStatus.Pending; public string? ProviderReference { get; private set; } public string? FailureReason { get; private set; } public IReadOnlyCollection<Refund> Refunds => _refunds.AsReadOnly(); public DateTimeOffset CreatedAt { get; private set; } public DateTimeOffset UpdatedAt { get; private set; }
    public static Payment Create(Guid orderId, decimal amount, PaymentMethod method) { if (orderId == Guid.Empty || amount <= 0) throw new DomainException("Order and positive payment amount are required."); return new Payment(Guid.NewGuid(), orderId, decimal.Round(amount, 2), method); }
    public void StartProcessing() { if (Status != PaymentStatus.Pending) throw new DomainException("Only pending payments can be processed."); Status = PaymentStatus.Processing; Touch(); Raise(new PaymentRequestedDomainEvent(Guid.NewGuid(), UpdatedAt, Id, OrderId, Amount)); }
    public void Succeed(string reference) { if (Status != PaymentStatus.Processing) throw new DomainException("Payment is not processing."); if (string.IsNullOrWhiteSpace(reference)) throw new DomainException("Provider reference is required."); ProviderReference = reference.Trim(); Status = PaymentStatus.Succeeded; Touch(); Raise(new PaymentSucceededDomainEvent(Guid.NewGuid(), UpdatedAt, Id, OrderId, Amount)); }
    public void Fail(string reason) { if (Status != PaymentStatus.Processing) throw new DomainException("Payment is not processing."); FailureReason = string.IsNullOrWhiteSpace(reason) ? "Payment gateway rejected the payment." : reason.Trim(); Status = PaymentStatus.Failed; Touch(); Raise(new PaymentFailedDomainEvent(Guid.NewGuid(), UpdatedAt, Id, OrderId, FailureReason)); }
    public Refund Refund(decimal amount, string reason) { if (Status != PaymentStatus.Succeeded) throw new DomainException("Only successful payments can be refunded."); if (_refunds.Sum(x => x.Amount) + amount > Amount) throw new DomainException("Refund exceeds the captured amount."); var refund = Payments.Refund.Create(amount, reason); _refunds.Add(refund); if (_refunds.Sum(x => x.Amount) == Amount) Status = PaymentStatus.Refunded; Touch(); return refund; }
    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
