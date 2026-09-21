using OrderFlow.Catalog.Domain.Common;
using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Domain.UnitTests.Products;

public sealed class ProductTests
{
    private static readonly Guid CategoryId = Guid.NewGuid();

    [Fact]
    public void Create_initializes_draft_and_raises_created_event()
    {
        var product = Product.Create(" sku-1 ", "Notebook", "Ruled", 12.50m, CategoryId);

        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal("SKU-1", product.Sku.Value);
        Assert.Equal(12.50m, product.Price.Amount);
        Assert.Equal(ProductStatus.Draft, product.Status);
        Assert.Contains(product.DomainEvents, x => x is ProductCreatedDomainEvent);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_rejects_blank_name(string name) =>
        Assert.Throws<DomainException>(() => Product.Create("SKU-1", name, null, 10m, CategoryId));

    [Fact]
    public void ChangePrice_raises_event_and_archive_blocks_mutation()
    {
        var product = Product.Create("SKU-1", "Notebook", null, 10m, CategoryId);
        product.ClearDomainEvents();
        product.ChangePrice(15m);
        product.Archive();

        Assert.Contains(product.DomainEvents, x => x is ProductPriceChangedDomainEvent);
        Assert.Equal(ProductStatus.Archived, product.Status);
        Assert.Throws<DomainException>(() => product.ChangePrice(20m));
    }
}
