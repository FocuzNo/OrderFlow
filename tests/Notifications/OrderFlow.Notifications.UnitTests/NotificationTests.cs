using OrderFlow.Notifications.Domain.Common;
using OrderFlow.Notifications.Domain.Notifications;

namespace OrderFlow.Notifications.UnitTests;

public sealed class NotificationTests
{
    [Fact]
    public void RecordSuccess_ShouldRequireSendingState()
    {
        var notification = Notification.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "OrderRecorded",
            "user@example.test",
            "Subject",
            "Message",
            NotificationChannel.Email
        );
        Assert.Throws<DomainException>(() => notification.RecordSuccess());
        notification.StartSending();
        notification.RecordSuccess();
        Assert.Equal(NotificationStatus.Sent, notification.Status);
        Assert.Single(notification.Attempts);
    }

    [Fact]
    public void Create_ShouldRejectMissingOrder() =>
        Assert.Throws<DomainException>(() =>
            Notification.Create(
                Guid.Empty,
                Guid.NewGuid(),
                "OrderRecorded",
                "user@example.test",
                "Subject",
                "Message",
                NotificationChannel.Email
            )
        );

    [Fact]
    public void Create_ShouldRejectInvalidRecipient() =>
        Assert.Throws<DomainException>(() =>
            Notification.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "OrderRecorded",
                "not-email",
                "Subject",
                "Message",
                NotificationChannel.Email
            )
        );
}
