namespace OrderFlow.Payments.Infrastructure.Messaging;

public sealed class KafkaOptions
{
    public const string SectionName = "Kafka";
    public string BootstrapServers { get; init; } = string.Empty;

    public string ConsumerGroup { get; init; } = "orderflow.payments.v1";

    public int MaxRetries { get; init; } = 5;

    public int OutboxBatchSize { get; init; } = 50;
}
