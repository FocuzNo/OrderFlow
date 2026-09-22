using OrderFlow.Payments.Domain.Payments;

namespace OrderFlow.Payments.Infrastructure.Persistence.Configurations;

public sealed class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.ToTable("refunds");
        builder.HasKey(candidate => candidate.Id);
        builder.Property(candidate => candidate.Id).ValueGeneratedNever();
        builder.Property(candidate => candidate.Amount).HasPrecision(18, 2);
        builder.Property(candidate => candidate.Reason).HasMaxLength(1000);
    }
}
