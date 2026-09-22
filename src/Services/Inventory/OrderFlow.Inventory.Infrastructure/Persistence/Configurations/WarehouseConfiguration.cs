using OrderFlow.Inventory.Domain.Warehouses;

namespace OrderFlow.Inventory.Infrastructure.Persistence.Configurations;

public sealed class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("warehouses");
        builder.HasKey(candidate => candidate.Id);
        builder.Property(candidate => candidate.Name).HasMaxLength(120);
        builder.Property(candidate => candidate.Location).HasMaxLength(500);
    }
}
