using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Catalog.Domain.Categories;

namespace OrderFlow.Catalog.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.ToTable("categories");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(Category.MaxNameLength);
        b.Property(x => x.Description).HasMaxLength(1000);
        b.HasIndex(x => x.Name).IsUnique();
    }
}
