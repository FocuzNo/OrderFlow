using Confluent.Kafka;
using Testcontainers.Kafka;

namespace OrderFlow.Infrastructure.IntegrationTests;

public sealed class KafkaRoundTripTests
{
    [DockerFact]
    public async Task Producer_and_manual_commit_consumer_preserve_the_message_key()
    {
        await using var kafka = new KafkaBuilder("confluentinc/cp-kafka:7.9.3").Build();
        await kafka.StartAsync();
        var topic = $"orderflow-test-{Guid.NewGuid():N}";
        var key = Guid.NewGuid().ToString("N");

        using (var producer = new ProducerBuilder<string, string>(new ProducerConfig { BootstrapServers = kafka.GetBootstrapAddress(), EnableIdempotence = true }).Build())
            await producer.ProduceAsync(topic, new Message<string, string> { Key = key, Value = "payload" });

        using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = kafka.GetBootstrapAddress(), GroupId = $"test-{Guid.NewGuid():N}",
            AutoOffsetReset = AutoOffsetReset.Earliest, EnableAutoCommit = false
        }).Build();
        consumer.Subscribe(topic);
        var result = consumer.Consume(TimeSpan.FromSeconds(30));

        Assert.NotNull(result);
        Assert.Equal(key, result.Message.Key);
        Assert.Equal("payload", result.Message.Value);
        consumer.Commit(result);
        consumer.Close();
    }
}
