using OrderFlow.Catalog.Domain.Common;
namespace OrderFlow.Catalog.Domain.Categories;
public sealed class Category : Entity
{
    public const int MaxNameLength = 120;
    private Category() { }
    private Category(Guid id, string name, string? description) : base(id) { Name = name; Description = description; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public static Category Create(string name, string? description) { Validate(name, description); return new Category(Guid.NewGuid(), name.Trim(), description?.Trim()); }
    public void Update(string name, string? description) { Validate(name, description); Name = name.Trim(); Description = description?.Trim(); }
    private static void Validate(string name, string? description) { if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > MaxNameLength) throw new DomainException("Category name must contain 1-120 characters."); if (description?.Length > 1000) throw new DomainException("Category description cannot exceed 1000 characters."); }
}
