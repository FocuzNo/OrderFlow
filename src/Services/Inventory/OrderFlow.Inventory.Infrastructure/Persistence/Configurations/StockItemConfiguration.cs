using OrderFlow.Inventory.Domain.Stock;

namespace OrderFlow.Inventory.Infrastructure.Persistence.Configurations;

public sealed class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.ToTable("stock_items");
        builder.HasKey(candidate => candidate.Id);
        builder.Property(candidate => candidate.Sku).HasMaxLength(64);
        builder.HasIndex(candidate => candidate.ProductId).IsUnique();
        builder.Ignore(candidate => candidate.AvailableQuantity);
        builder
            .HasMany(candidate => candidate.Reservations)
            .WithOne()
            .HasForeignKey(candidate => candidate.StockItemId);
        builder.Property(candidate => candidate.Version).IsRowVersion();
    }
}
