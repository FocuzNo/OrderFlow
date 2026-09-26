using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace OrderFlow.Ordering.Infrastructure.Messaging.Kafka;

public sealed class KafkaPublisher(
    IProducer<string, string> producer,
    ILogger<KafkaPublisher> logger
) : IKafkaPublisher
{
    public async Task PublishAsync(
        string topic,
        string key,
        string content,
        CancellationToken cancellationToken
    )
    {
        var message = new Message<string, string>
        {
            Key = key,
            Value = content,
        };

        var deliveryResult = await producer.ProduceAsync(
            topic,
            message,
            cancellationToken
        );

        logger.LogInformation(
            "Published Kafka message with key {MessageKey} to topic {Topic}, partition {Partition}, offset {Offset}",
            key,
            deliveryResult.Topic,
            deliveryResult.Partition.Value,
            deliveryResult.Offset.Value
        );
    }
}
