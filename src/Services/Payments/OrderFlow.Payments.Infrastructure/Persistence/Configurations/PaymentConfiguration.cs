using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Payments.Domain.Payments;

namespace OrderFlow.Payments.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder
            .Property(x => x.Method)
            .HasConversion(x => x.Value, x => PaymentMethod.FromValue(x));
        builder
            .Property(x => x.Status)
            .HasConversion(x => x.Value, x => PaymentStatus.FromValue(x));
        builder.Property(x => x.ProviderReference).HasMaxLength(200);
        builder.Property(x => x.FailureReason).HasMaxLength(1000);
        builder.HasIndex(x => x.OrderId).IsUnique();
        builder.Ignore(x => x.DomainEvents);
        builder.HasMany(x => x.Refunds).WithOne().HasForeignKey("PaymentId");
    }
}
