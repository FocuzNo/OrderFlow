using OrderFlow.Catalog.Domain.Categories;

namespace OrderFlow.Catalog.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");
        builder.HasKey(candidate => candidate.Id);
        builder.Property(candidate => candidate.Name).HasMaxLength(Category.MaxNameLength);
        builder.Property(candidate => candidate.Description).HasMaxLength(1000);
        builder.HasIndex(candidate => candidate.Name).IsUnique();
    }
}
