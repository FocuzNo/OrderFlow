using OrderFlow.Ordering.Domain.Orders;

namespace OrderFlow.Ordering.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(candidate => candidate.Id);
        builder
            .Property(candidate => candidate.CustomerEmail)
            .HasConversion(
                candidate => candidate.Value,
                candidate => EmailAddress.Create(candidate)
            )
            .HasMaxLength(320);
        builder
            .Property(candidate => candidate.Status)
            .HasConversion(
                candidate => candidate.Value,
                candidate => OrderStatus.FromValue(candidate)
            );
        builder.Ignore(candidate => candidate.TotalAmount);
        builder.OwnsOne(
            candidate => candidate.ShippingAddress,
            address =>
            {
                address
                    .Property(candidate => candidate.Line1)
                    .HasColumnName("shipping_line1")
                    .HasMaxLength(300);
                address
                    .Property(candidate => candidate.City)
                    .HasColumnName("shipping_city")
                    .HasMaxLength(120);
                address
                    .Property(candidate => candidate.PostalCode)
                    .HasColumnName("shipping_postal_code")
                    .HasMaxLength(30);
                address
                    .Property(candidate => candidate.Country)
                    .HasColumnName("shipping_country")
                    .HasMaxLength(100);
            }
        );
        builder
            .HasMany(candidate => candidate.Items)
            .WithOne()
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(candidate => new { candidate.CustomerId, candidate.CreatedAt });
        builder.Property(candidate => candidate.Version).IsRowVersion();
    }
}
