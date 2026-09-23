using OrderFlow.Notifications.Domain.Notifications;

namespace OrderFlow.Notifications.Infrastructure.Persistence.Configurations;

public sealed class NotificationDeliveryAttemptConfiguration
    : IEntityTypeConfiguration<NotificationDeliveryAttempt>
{
    public void Configure(EntityTypeBuilder<NotificationDeliveryAttempt> builder)
    {
        builder.ToTable("notification_delivery_attempts");
        builder.HasKey(candidate => candidate.Id);
        builder.Property(candidate => candidate.Id).ValueGeneratedNever();
        builder.Property(candidate => candidate.Error).HasMaxLength(2000);
    }
}
