using OrderFlow.Payments.Domain.Common;

namespace OrderFlow.Payments.Domain.Payments;

public sealed class Refund : Entity
{
    private Refund() { }

    private Refund(Guid id, decimal amount, string reason)
        : base(id)
    {
        Amount = amount;
        Reason = reason;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public decimal Amount { get; private set; }

    public string Reason { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public static Refund Create(decimal amount, string reason)
    {
        if (amount <= 0 || string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Refund amount and reason are required.");
        return new Refund(Guid.NewGuid(), decimal.Round(amount, 2), reason.Trim());
    }
}
