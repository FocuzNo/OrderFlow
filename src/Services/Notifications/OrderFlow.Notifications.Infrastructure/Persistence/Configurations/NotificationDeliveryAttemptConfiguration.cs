using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Notifications.Domain.Notifications;

namespace OrderFlow.Notifications.Infrastructure.Persistence.Configurations;

public sealed class NotificationDeliveryAttemptConfiguration
    : IEntityTypeConfiguration<NotificationDeliveryAttempt>
{
    public void Configure(EntityTypeBuilder<NotificationDeliveryAttempt> builder)
    {
        builder.ToTable("notification_delivery_attempts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Error).HasMaxLength(2000);
    }
}
