using OrderFlow.Catalog.Domain.Common;
namespace OrderFlow.Catalog.Domain.Products;
public readonly record struct Sku
{
    public const int MaxLength = 64;
    public string Value { get; }
    private Sku(string value) => Value = value;
    public static Sku Create(string value)
    { if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > MaxLength) throw new DomainException("SKU must contain 1-64 characters."); return new Sku(value.Trim().ToUpperInvariant()); }
    public override string ToString() => Value;
}
