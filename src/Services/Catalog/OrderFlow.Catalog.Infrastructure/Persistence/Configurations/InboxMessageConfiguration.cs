namespace OrderFlow.Catalog.Infrastructure.Persistence.Configurations;

public sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("inbox_messages");
        builder.HasKey(candidate => new { candidate.Id, candidate.Consumer });
        builder.Property(candidate => candidate.Consumer).HasMaxLength(200);
    }
}
