using OrderFlow.Inventory.Domain.Common;
namespace OrderFlow.Inventory.Domain.Warehouses;
public sealed class Warehouse : Entity
{
    private Warehouse() { }
    private Warehouse(Guid id, string name, string location) : base(id) { Name = name; Location = location; }
    public string Name { get; private set; } = string.Empty; public string Location { get; private set; } = string.Empty;
    public static Warehouse Create(string name, string location) { if (string.IsNullOrWhiteSpace(name) || name.Length > 120) throw new DomainException("Warehouse name is required and cannot exceed 120 characters."); if (string.IsNullOrWhiteSpace(location) || location.Length > 500) throw new DomainException("Warehouse location is required and cannot exceed 500 characters."); return new Warehouse(Guid.NewGuid(), name.Trim(), location.Trim()); }
}
