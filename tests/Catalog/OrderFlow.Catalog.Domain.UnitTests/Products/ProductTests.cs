using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Domain.UnitTests.Products;

public sealed class ProductTests
{
    [Fact]
    public void Create_initializes_valid_product_with_identity_and_utc_timestamps()
    {
        var beforeCreation = DateTimeOffset.UtcNow;

        var product = Product.Create("Notebook", null, 12.50m);

        var afterCreation = DateTimeOffset.UtcNow;
        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal("Notebook", product.Name);
        Assert.Null(product.Description);
        Assert.Equal(12.50m, product.Price);
        Assert.InRange(product.CreatedAt, beforeCreation, afterCreation);
        Assert.Equal(product.CreatedAt, product.UpdatedAt);
        Assert.Equal(TimeSpan.Zero, product.CreatedAt.Offset);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" \t ")]
    public void Create_rejects_empty_or_whitespace_name(string name)
    {
        Assert.Throws<ArgumentException>(() => Product.Create(name, null, 10m));
    }

    [Fact]
    public void Create_rejects_name_over_maximum_length()
    {
        var name = new string('n', Product.MaxNameLength + 1);

        Assert.Throws<ArgumentException>(() => Product.Create(name, null, 10m));
    }

    [Fact]
    public void Create_rejects_description_over_maximum_length()
    {
        var description = new string('d', Product.MaxDescriptionLength + 1);

        Assert.Throws<ArgumentException>(() => Product.Create("Notebook", description, 10m));
    }

    [Fact]
    public void Create_rejects_negative_price()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Product.Create("Notebook", null, -0.01m));
    }

    [Fact]
    public void Update_changes_mutable_state_and_refreshes_timestamp()
    {
        var product = Product.Create("Notebook", "Old", 10m);
        var originalId = product.Id;
        var originalCreatedAt = product.CreatedAt;
        var originalUpdatedAt = product.UpdatedAt;
        Assert.True(SpinWait.SpinUntil(
            () => DateTimeOffset.UtcNow > originalUpdatedAt,
            TimeSpan.FromSeconds(1)));

        product.Update("Pen", "New", 2.50m);

        Assert.Equal(originalId, product.Id);
        Assert.Equal("Pen", product.Name);
        Assert.Equal("New", product.Description);
        Assert.Equal(2.50m, product.Price);
        Assert.Equal(originalCreatedAt, product.CreatedAt);
        Assert.True(product.UpdatedAt > originalUpdatedAt);
        Assert.Equal(TimeSpan.Zero, product.UpdatedAt.Offset);
    }

    [Fact]
    public void Invalid_update_does_not_change_any_product_state()
    {
        var product = Product.Create("Notebook", "Original", 10m);
        var originalUpdatedAt = product.UpdatedAt;

        Assert.Throws<ArgumentOutOfRangeException>(() => product.Update("Pen", "New", -1m));
        Assert.Throws<ArgumentException>(() => product.Update(" ", "New", 2m));
        Assert.Throws<ArgumentException>(() => product.Update("Pen", new string('d', Product.MaxDescriptionLength + 1), 2m));

        Assert.Equal("Notebook", product.Name);
        Assert.Equal("Original", product.Description);
        Assert.Equal(10m, product.Price);
        Assert.Equal(originalUpdatedAt, product.UpdatedAt);
    }
}
