using OrderFlow.Catalog.Domain.Common;

namespace OrderFlow.Catalog.Domain.Products;

public sealed class Product : AggregateRoot
{
    public const int MaxNameLength = 200;
    public const int MaxDescriptionLength = 2000;

    private Product() { }

    private Product(
        Guid id,
        Sku sku,
        string name,
        string? description,
        Money price,
        Guid categoryId,
        DateTimeOffset now
    )
        : base(id)
    {
        Sku = sku;
        Name = name;
        Description = description;
        Price = price;
        CategoryId = categoryId;
        Status = ProductStatus.Draft;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public Sku Sku { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public Money Price { get; private set; }

    public Guid CategoryId { get; private set; }

    public ProductStatus Status { get; private set; } = ProductStatus.Draft;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public uint Version { get; private set; }

    public static Product Create(
        string sku,
        string name,
        string? description,
        decimal price,
        Guid categoryId
    )
    {
        Validate(name, description, categoryId);
        var now = DateTimeOffset.UtcNow;
        var product = new Product(
            Guid.NewGuid(),
            Sku.Create(sku),
            name.Trim(),
            description?.Trim(),
            Money.From(price),
            categoryId,
            now
        );

        return product;
    }

    public void Update(string name, string? description, Guid categoryId)
    {
        EnsureNotArchived();
        Validate(name, description, categoryId);
        Name = name.Trim();
        Description = description?.Trim();
        CategoryId = categoryId;
        Touch();
    }

    public void ChangePrice(decimal price)
    {
        EnsureNotArchived();
        Price = Money.From(price);
        Touch();
    }

    public void Activate()
    {
        EnsureNotArchived();
        Status = ProductStatus.Active;
        Touch();
    }

    public void Deactivate()
    {
        EnsureNotArchived();
        Status = ProductStatus.Inactive;
        Touch();
    }

    public void Archive()
    {
        if (Status == ProductStatus.Archived)
            return;
        Status = ProductStatus.Archived;
        Touch();
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;

    private void EnsureNotArchived()
    {
        if (Status == ProductStatus.Archived)
            throw new DomainException("Archived products cannot be changed.");
    }

    private static void Validate(string name, string? description, Guid categoryId)
    {
        if (categoryId == Guid.Empty)
            throw new DomainException("Category is required.");
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > MaxNameLength)
            throw new DomainException("Product name must contain 1-200 characters.");
        if (description?.Length > MaxDescriptionLength)
            throw new DomainException("Product description cannot exceed 2000 characters.");
    }
}
