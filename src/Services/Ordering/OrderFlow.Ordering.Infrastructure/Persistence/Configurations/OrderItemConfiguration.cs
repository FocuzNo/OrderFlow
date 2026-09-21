using OrderFlow.Ordering.Domain.Orders;

namespace OrderFlow.Ordering.Infrastructure.Persistence.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");
        builder.HasKey(candidate => candidate.Id);
        builder.Property(candidate => candidate.ProductName).HasMaxLength(200);
        builder.Property(candidate => candidate.UnitPrice).HasPrecision(18, 2);
        builder.Ignore(candidate => candidate.Total);
    }
}
