using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrderFlow.Ordering.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessageConfiguration
    : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(
        EntityTypeBuilder<OutboxMessage> builder
    )
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(message => message.Id);

        builder
            .Property(message => message.Topic)
            .HasMaxLength(300)
            .IsRequired();

        builder
            .Property(message => message.Type)
            .HasMaxLength(500)
            .IsRequired();

        builder
            .Property(message => message.Key)
            .HasMaxLength(200)
            .IsRequired();

        builder
            .Property(message => message.Content)
            .IsRequired();

        builder
            .Property(message => message.OccurredAt)
            .IsRequired();

        builder.Property(
            message => message.ProcessedAt
        );

        builder
            .Property(message => message.RetryCount)
            .IsRequired();

        builder
            .Property(message => message.Error)
            .HasMaxLength(2000);

        builder.HasIndex(
            message => new
            {
                message.ProcessedAt,
                message.OccurredAt,
            }
        );
    }
}
