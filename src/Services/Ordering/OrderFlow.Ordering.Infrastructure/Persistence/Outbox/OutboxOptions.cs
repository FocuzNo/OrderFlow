namespace OrderFlow.Ordering.Infrastructure.Messaging.Outbox;

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    public int BatchSize { get; init; } = 20;

    public int PollingIntervalMilliseconds { get; init; } = 2000;

    public int MaxRetries { get; init; } = 10;
}
