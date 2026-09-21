using OrderFlow.Inventory.Domain.Stock;

namespace OrderFlow.Inventory.Infrastructure.Persistence.Configurations;

public sealed class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.ToTable("stock_items");
        builder.HasKey(candidate => candidate.Id);
        builder.Property(candidate => candidate.Sku).HasMaxLength(64);
        builder
            .HasIndex(candidate => new { candidate.ProductId, candidate.WarehouseId })
            .IsUnique();
        builder.Ignore(candidate => candidate.DomainEvents);
        builder.Ignore(candidate => candidate.AvailableQuantity);
        builder
            .HasMany(candidate => candidate.Reservations)
            .WithOne()
            .HasForeignKey(candidate => candidate.StockItemId);
        builder.Property(candidate => candidate.Version).IsRowVersion();
    }
}
