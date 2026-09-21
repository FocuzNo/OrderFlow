namespace OrderFlow.Catalog.Domain.Products;

public sealed class Product
{
    public const int MaxNameLength = 200;
    public const int MaxDescriptionLength = 2_000;

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private Product(
        Guid id,
        string name,
        string? description,
        decimal price,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Product ID cannot be empty.", nameof(id));
        }

        Id = id;
        Name = name;
        Description = description;
        Price = price;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public static Product Create(string name, string? description, decimal price)
    {
        Validate(name, description, price);

        return new Product(Guid.NewGuid(), name, description, price, DateTimeOffset.UtcNow);
    }

    public void Update(string name, string? description, decimal price)
    {
        Validate(name, description, price);

        Name = name;
        Description = description;
        Price = price;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static void Validate(string name, string? description, decimal price)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (name.Length > MaxNameLength)
        {
            throw new ArgumentException($"Product name cannot exceed {MaxNameLength} characters.", nameof(name));
        }

        if (description?.Length > MaxDescriptionLength)
        {
            throw new ArgumentException(
                $"Product description cannot exceed {MaxDescriptionLength} characters.",
                nameof(description));
        }

        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Product price cannot be negative.");
        }
    }
}
