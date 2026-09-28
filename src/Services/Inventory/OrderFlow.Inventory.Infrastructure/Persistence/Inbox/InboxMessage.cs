namespace OrderFlow.Inventory.Infrastructure.Persistence.Inbox;

public sealed class InboxMessage
{
    private InboxMessage()
    {
    }

    public Guid Id { get; private set; }

    public string Type { get; private set; } =
        string.Empty;

    public DateTimeOffset ProcessedAt { get; private set; }

    public static InboxMessage Create(
        Guid id,
        string type,
        DateTimeOffset processedAt
    )
    {
        return new InboxMessage
        {
            Id = id,
            Type = type,
            ProcessedAt = processedAt,
        };
    }
}

