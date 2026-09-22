using OrderFlow.Catalog.Domain.Common;
using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Domain.UnitTests.Products;

public sealed class ProductTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_ShouldRejectBlankName(string name) =>
        Assert.Throws<DomainException>(() => Product.Create("SKU", name, null, 1, Guid.NewGuid()));

    [Fact]
    public void ChangePrice_ShouldRejectArchivedMutation()
    {
        var product = Product.Create("SKU", "Notebook", null, 1, Guid.NewGuid());
        product.ChangePrice(15);
        Assert.Equal(15, product.Price.Amount);
        product.Archive();
        Assert.Throws<DomainException>(() => product.ChangePrice(20));
    }

    [Fact]
    public void Create_ShouldRejectNegativePrice() =>
        Assert.Throws<DomainException>(() =>
            Product.Create("SKU", "Notebook", null, -1, Guid.NewGuid())
        );
}
