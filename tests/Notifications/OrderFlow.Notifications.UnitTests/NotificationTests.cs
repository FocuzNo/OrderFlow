using OrderFlow.Notifications.Domain.Common;
using OrderFlow.Notifications.Domain.Notifications;

namespace OrderFlow.Notifications.UnitTests;

public sealed class NotificationTests
{
    [Fact]
    public void Successful_delivery_records_attempt_and_timestamp()
    {
        var notification = Notification.Create(
            "user@example.test",
            "Order confirmed",
            "Your order is ready.",
            NotificationChannel.Email
        );
        notification.StartSending();
        notification.RecordSuccess();

        Assert.Equal(NotificationStatus.Sent, notification.Status);
        Assert.NotNull(notification.SentAt);
        Assert.Single(notification.Attempts);
    }

    [Fact]
    public void Invalid_recipient_is_rejected() =>
        Assert.Throws<DomainException>(() =>
            Notification.Create("not-email", "Subject", "Body", NotificationChannel.Email)
        );
}
