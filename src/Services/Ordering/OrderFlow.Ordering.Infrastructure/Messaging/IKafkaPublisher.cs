namespace OrderFlow.Ordering.Infrastructure.Messaging;
public interface IKafkaPublisher { Task PublishAsync(string topic,string key,string content,CancellationToken cancellationToken); }
