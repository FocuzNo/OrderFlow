namespace OrderFlow.Notifications.Infrastructure.Persistence;

public sealed class InboxMessage
{
    public Guid Id { get; set; }

    public string Consumer { get; set; } = string.Empty;

    public DateTimeOffset ProcessedOnUtc { get; set; }

    public string? Error { get; set; }
}
