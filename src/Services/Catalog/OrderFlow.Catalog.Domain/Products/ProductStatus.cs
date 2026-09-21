using Ardalis.SmartEnum;
namespace OrderFlow.Catalog.Domain.Products;
public sealed class ProductStatus : SmartEnum<ProductStatus>
{
    public static readonly ProductStatus Draft = new(nameof(Draft), 0);
    public static readonly ProductStatus Active = new(nameof(Active), 1);
    public static readonly ProductStatus Inactive = new(nameof(Inactive), 2);
    public static readonly ProductStatus Archived = new(nameof(Archived), 3);
    private ProductStatus(string name, int value) : base(name, value) { }
}
