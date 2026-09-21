using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Inventory.Domain.Stock;

namespace OrderFlow.Inventory.Infrastructure.Persistence.Configurations;

public sealed class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.ToTable("stock_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Sku).HasMaxLength(64);
        builder.HasIndex(x => new { x.ProductId, x.WarehouseId }).IsUnique();
        builder.Ignore(x => x.DomainEvents);
        builder.Ignore(x => x.AvailableQuantity);
        builder.HasMany(x => x.Reservations).WithOne().HasForeignKey(x => x.StockItemId);
        builder.Property(x => x.Version).IsRowVersion();
    }
}
