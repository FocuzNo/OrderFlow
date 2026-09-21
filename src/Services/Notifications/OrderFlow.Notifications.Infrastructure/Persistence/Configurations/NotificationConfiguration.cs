using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Notifications.Domain.Notifications;

namespace OrderFlow.Notifications.Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Recipient).HasMaxLength(320);
        builder.Property(x => x.Subject).HasMaxLength(250);
        builder.Property(x => x.Body).HasMaxLength(10000);
        builder
            .Property(x => x.Channel)
            .HasConversion(x => x.Value, x => NotificationChannel.FromValue(x));
        builder
            .Property(x => x.Status)
            .HasConversion(x => x.Value, x => NotificationStatus.FromValue(x));
        builder.Ignore(x => x.DomainEvents);
        builder.HasMany(x => x.Attempts).WithOne().HasForeignKey("NotificationId");
        builder.HasIndex(x => new { x.Recipient, x.CreatedAt });
    }
}
