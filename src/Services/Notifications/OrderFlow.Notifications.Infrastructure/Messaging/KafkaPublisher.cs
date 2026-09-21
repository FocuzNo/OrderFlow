using System.Diagnostics; using Confluent.Kafka; using Microsoft.Extensions.Options;
namespace OrderFlow.Notifications.Infrastructure.Messaging;
public sealed class KafkaPublisher:IKafkaPublisher,IDisposable
{
    public static readonly ActivitySource ActivitySource=new("OrderFlow.Notifications.Kafka"); private readonly IProducer<string,string> _producer;
    public KafkaPublisher(IOptions<KafkaOptions> options)=>_producer=new ProducerBuilder<string,string>(new ProducerConfig{BootstrapServers=options.Value.BootstrapServers,EnableIdempotence=true,Acks=Acks.All}).Build();
    public async Task PublishAsync(string topic,string key,string content,CancellationToken ct){using var activity=ActivitySource.StartActivity("kafka publish",ActivityKind.Producer);activity?.SetTag("messaging.destination.name",topic);activity?.SetTag("messaging.kafka.message.key",key);await _producer.ProduceAsync(topic,new Message<string,string>{Key=key,Value=content},ct);}
    public void Dispose()=>_producer.Dispose();
}
