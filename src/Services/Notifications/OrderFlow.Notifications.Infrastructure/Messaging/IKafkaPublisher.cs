namespace OrderFlow.Notifications.Infrastructure.Messaging;
public interface IKafkaPublisher { Task PublishAsync(string topic,string key,string content,CancellationToken cancellationToken); }
