namespace OrderFlow.Notifications.Infrastructure.Messaging;

public sealed class KafkaOptions
{
    public const string SectionName = "Kafka";
    public string BootstrapServers { get; init; } = string.Empty;

    public string ConsumerGroup { get; init; } = "orderflow.notifications.v1";

    public int MaxRetries { get; init; } = 5;

    public int OutboxBatchSize { get; init; } = 50;
}
