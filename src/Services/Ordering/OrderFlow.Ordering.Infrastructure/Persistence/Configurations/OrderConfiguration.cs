using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Ordering.Domain.Orders;

namespace OrderFlow.Ordering.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(x => x.Id);
        builder
            .Property(x => x.CustomerEmail)
            .HasConversion(x => x.Value, x => EmailAddress.Create(x))
            .HasMaxLength(320);
        builder.Property(x => x.Status).HasConversion(x => x.Value, x => OrderStatus.FromValue(x));
        builder.Ignore(x => x.DomainEvents);
        builder.Ignore(x => x.TotalAmount);
        builder.OwnsOne(
            x => x.ShippingAddress,
            address =>
            {
                address.Property(x => x.Line1).HasColumnName("shipping_line1").HasMaxLength(300);
                address.Property(x => x.City).HasColumnName("shipping_city").HasMaxLength(120);
                address
                    .Property(x => x.PostalCode)
                    .HasColumnName("shipping_postal_code")
                    .HasMaxLength(30);
                address
                    .Property(x => x.Country)
                    .HasColumnName("shipping_country")
                    .HasMaxLength(100);
            }
        );
        builder
            .HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.CustomerId, x.CreatedAt });
        builder.Property(x => x.Version).IsRowVersion();
    }
}
