using Ardalis.SmartEnum;
namespace OrderFlow.Notifications.Domain.Notifications;
public sealed class NotificationStatus : SmartEnum<NotificationStatus>
{
    public static readonly NotificationStatus Pending = new(nameof(Pending), 0); public static readonly NotificationStatus Sending = new(nameof(Sending), 1); public static readonly NotificationStatus Sent = new(nameof(Sent), 2); public static readonly NotificationStatus Failed = new(nameof(Failed), 3);
    private NotificationStatus(string name, int value) : base(name, value) { }
}
