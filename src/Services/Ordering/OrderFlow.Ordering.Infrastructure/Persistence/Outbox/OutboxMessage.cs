namespace OrderFlow.Ordering.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessage
{
    private OutboxMessage()
    {
    }

    public Guid Id { get; private set; }

    public string Topic { get; private set; } = string.Empty;

    public string Type { get; private set; } = string.Empty;

    public string Key { get; private set; } = string.Empty;

    public string Content { get; private set; } = string.Empty;

    public DateTimeOffset OccurredAt { get; private set; }

    public DateTimeOffset? ProcessedAt { get; private set; }

    public int RetryCount { get; private set; }

    public string? Error { get; private set; }

    public static OutboxMessage Create(
        Guid id,
        string topic,
        string type,
        string key,
        string content,
        DateTimeOffset occurredAt
    )
    {
        return new OutboxMessage
        {
            Id = id,
            Topic = topic,
            Type = type,
            Key = key,
            Content = content,
            OccurredAt = occurredAt,
            ProcessedAt = null,
            RetryCount = 0,
            Error = null,
        };
    }

    public void MarkAsProcessed(
        DateTimeOffset processedAt
    )
    {
        ProcessedAt = processedAt;
        Error = null;
    }

    public void MarkAsFailed(
        string error
    )
    {
        RetryCount++;
        Error = error;
    }
}
