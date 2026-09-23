using OrderFlow.Notifications.Domain.Notifications;

namespace OrderFlow.Notifications.Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");
        builder.Property(notification => notification.NotificationType).HasMaxLength(100);
        builder.HasIndex(notification => notification.OrderId);
        builder.HasKey(candidate => candidate.Id);
        builder.Property(candidate => candidate.Recipient).HasMaxLength(320);
        builder.Property(candidate => candidate.Subject).HasMaxLength(250);
        builder.Property(candidate => candidate.Body).HasMaxLength(10000);
        builder
            .Property(candidate => candidate.Channel)
            .HasConversion(
                candidate => candidate.Value,
                candidate => NotificationChannel.FromValue(candidate)
            );
        builder
            .Property(candidate => candidate.Status)
            .HasConversion(
                candidate => candidate.Value,
                candidate => NotificationStatus.FromValue(candidate)
            );
        builder.HasMany(candidate => candidate.Attempts).WithOne().HasForeignKey("NotificationId");
        builder.HasIndex(candidate => new { candidate.Recipient, candidate.CreatedAt });
    }
}
