namespace OrderFlow.Payments.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(candidate => candidate.Id);
        builder.Property(candidate => candidate.Type).HasMaxLength(250);
        builder.Property(candidate => candidate.Content).HasColumnType("jsonb");
        builder.Property(candidate => candidate.AggregateId).HasMaxLength(100);
        builder.Property(candidate => candidate.Error).HasMaxLength(2000);
        builder.HasIndex(candidate => new { candidate.ProcessedOnUtc, candidate.OccurredOnUtc });
    }
}
