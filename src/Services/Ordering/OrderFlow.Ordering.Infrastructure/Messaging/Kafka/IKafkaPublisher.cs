namespace OrderFlow.Ordering.Infrastructure.Messaging.Kafka;

public interface IKafkaPublisher
{
    Task PublishAsync(
        string topic,
        string key,
        string content,
        CancellationToken cancellationToken
    );
}
