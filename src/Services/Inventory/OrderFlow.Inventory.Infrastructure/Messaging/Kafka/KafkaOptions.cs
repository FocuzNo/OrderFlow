namespace OrderFlow.Inventory.Infrastructure.Messaging.Kafka;

public sealed class KafkaOptions
{
    public const string SectionName = "Kafka";

    public required string BootstrapServers { get; init; }

    public required string ConsumerGroup { get; init; }

    public required string OrderCreatedTopic { get; init; }

    public required string OrderCreatedDeadLetterTopic { get; init; }

    public required string InventoryReservedTopic { get; init; }

    public required string InventoryReservationFailedTopic { get; init; }

    public int ProcessingMaxRetries { get; init; } = 3;

    public int RetryDelayMilliseconds { get; init; } = 1000;

    public int MaxPollIntervalMilliseconds { get; init; } = 300000;
}
