namespace OrderFlow.Inventory.Infrastructure.Messaging.Kafka;

public sealed class KafkaOptions
{
    public const string SectionName = "Kafka";

    public required string BootstrapServers { get; init; }

    public required string OrderCreatedTopic { get; init; }

    public required string ConsumerGroup { get; init; }
}
