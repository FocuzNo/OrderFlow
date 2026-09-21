using Ardalis.SmartEnum;

namespace OrderFlow.Notifications.Domain.Notifications;

public sealed class NotificationChannel : SmartEnum<NotificationChannel>
{
    public static readonly NotificationChannel Email = new(nameof(Email), 1);

    private NotificationChannel(string name, int value)
        : base(name, value) { }
}
