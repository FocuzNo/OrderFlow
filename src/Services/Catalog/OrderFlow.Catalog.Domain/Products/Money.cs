using OrderFlow.Catalog.Domain.Common;
namespace OrderFlow.Catalog.Domain.Products;
public readonly record struct Money
{
    public decimal Amount { get; }
    private Money(decimal amount) => Amount = amount;
    public static Money From(decimal amount) => amount < 0 ? throw new DomainException("Price cannot be negative.") : new Money(decimal.Round(amount, 2));
}
