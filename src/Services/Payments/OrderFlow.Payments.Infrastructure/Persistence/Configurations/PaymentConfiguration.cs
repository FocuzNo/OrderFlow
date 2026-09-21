using OrderFlow.Payments.Domain.Payments;

namespace OrderFlow.Payments.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(candidate => candidate.Id);
        builder.Property(candidate => candidate.Amount).HasPrecision(18, 2);
        builder
            .Property(candidate => candidate.Method)
            .HasConversion(
                candidate => candidate.Value,
                candidate => PaymentMethod.FromValue(candidate)
            );
        builder
            .Property(candidate => candidate.Status)
            .HasConversion(
                candidate => candidate.Value,
                candidate => PaymentStatus.FromValue(candidate)
            );
        builder.Property(candidate => candidate.ProviderReference).HasMaxLength(200);
        builder.Property(candidate => candidate.FailureReason).HasMaxLength(1000);
        builder.HasIndex(candidate => candidate.OrderId).IsUnique();
        builder.Ignore(candidate => candidate.DomainEvents);
        builder.HasMany(candidate => candidate.Refunds).WithOne().HasForeignKey("PaymentId");
    }
}
