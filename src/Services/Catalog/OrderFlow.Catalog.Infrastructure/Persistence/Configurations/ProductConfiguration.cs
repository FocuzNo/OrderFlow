using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.HasKey(candidate => candidate.Id);
        builder
            .Property(candidate => candidate.Sku)
            .HasConversion(candidate => candidate.Value, candidate => Sku.Create(candidate))
            .HasMaxLength(Sku.MaxLength);
        builder.HasIndex(candidate => candidate.Sku).IsUnique();
        builder.Property(candidate => candidate.Name).HasMaxLength(Product.MaxNameLength);
        builder
            .Property(candidate => candidate.Description)
            .HasMaxLength(Product.MaxDescriptionLength);
        builder
            .Property(candidate => candidate.Price)
            .HasConversion(candidate => candidate.Amount, candidate => Money.From(candidate))
            .HasPrecision(18, 2);
        builder
            .Property(candidate => candidate.Status)
            .HasConversion(
                candidate => candidate.Value,
                candidate => ProductStatus.FromValue(candidate)
            );
        builder.HasIndex(candidate => candidate.CategoryId);
        builder.Ignore(candidate => candidate.DomainEvents);
        builder.Property(candidate => candidate.Version).IsRowVersion();
    }
}
