using Confluent.Kafka;

namespace OrderFlow.Inventory.Infrastructure.Messaging.Kafka;

public interface IKafkaPublisher
{
    Task PublishAsync(
        string topic,
        string key,
        string content,
        CancellationToken cancellationToken,
        Headers? headers = null
    );
}
