using OrderFlow.Notifications.Domain.Common;
namespace OrderFlow.Notifications.Domain.Notifications;
public sealed class NotificationDeliveryAttempt : Entity
{
    private NotificationDeliveryAttempt() { }
    private NotificationDeliveryAttempt(Guid id, bool succeeded, string? error) : base(id) { Succeeded = succeeded; Error = error; AttemptedAt = DateTimeOffset.UtcNow; }
    public bool Succeeded { get; private set; } public string? Error { get; private set; } public DateTimeOffset AttemptedAt { get; private set; }
    public static NotificationDeliveryAttempt Create(bool succeeded, string? error) => new(Guid.NewGuid(), succeeded, error);
}
