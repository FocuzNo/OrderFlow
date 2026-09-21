using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> b)
    {
        b.ToTable("products");
        b.HasKey(x => x.Id);
        b.Property(x => x.Sku)
            .HasConversion(x => x.Value, x => Sku.Create(x))
            .HasMaxLength(Sku.MaxLength);
        b.HasIndex(x => x.Sku).IsUnique();
        b.Property(x => x.Name).HasMaxLength(Product.MaxNameLength);
        b.Property(x => x.Description).HasMaxLength(Product.MaxDescriptionLength);
        b.Property(x => x.Price)
            .HasConversion(x => x.Amount, x => Money.From(x))
            .HasPrecision(18, 2);
        b.Property(x => x.Status).HasConversion(x => x.Value, x => ProductStatus.FromValue(x));
        b.HasIndex(x => x.CategoryId);
        b.Ignore(x => x.DomainEvents);
        b.Property(x => x.Version).IsRowVersion();
    }
}
