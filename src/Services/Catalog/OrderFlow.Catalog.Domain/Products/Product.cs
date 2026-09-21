using OrderFlow.Catalog.Domain;

namespace OrderFlow.Catalog.Domain.Products;

public sealed class Product : IHasDomainEvents
{
    private readonly List<DomainEvent> _domainEvents = [];
    public const int MaxNameLength = 200;
    public const int MaxDescriptionLength = 2_000;

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

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

        var product = new Product(Guid.NewGuid(), name.Trim(), description?.Trim(), price, DateTimeOffset.UtcNow);
        product._domainEvents.Add(new DomainEvent(Guid.NewGuid(), "catalog.product-created", product.Id, product.CreatedAt));
        return product;
    }

    public void Update(string name, string? description, decimal price)
    {
        Validate(name, description, price);

        Name = name.Trim();
        Description = description?.Trim();
        Price = price;
        UpdatedAt = DateTimeOffset.UtcNow;
        _domainEvents.Add(new DomainEvent(Guid.NewGuid(), "catalog.product-updated", Id, UpdatedAt));
    }

    public void MarkDeleted() =>
        _domainEvents.Add(new DomainEvent(Guid.NewGuid(), "catalog.product-deleted", Id, DateTimeOffset.UtcNow));

    public void ClearDomainEvents() => _domainEvents.Clear();

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
