using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderFlow.IntegrationEvents.Orders;
using OrderFlow.Ordering.Application.Abstractions.Messaging;
using OrderFlow.Ordering.Domain.Orders;

namespace OrderFlow.Ordering.Infrastructure.Messaging.Kafka.Producers;

public sealed class OrderCreatedKafkaPublisher(
    IProducer<string, string> producer,
    IOptions<KafkaOptions> options,
    ILogger<OrderCreatedKafkaPublisher> logger) : IOrderCreatedPublisher
{
    public async Task PublishAsync(
        Order order,
        CancellationToken cancellationToken)
    {
        var integrationEvent = new OrderCreatedIntegrationEvent(
            Guid.NewGuid(),
            order.CreatedAt,
            order.Id,
            [
                .. order.Items
                    .Select(item =>
                        new OrderCreatedItem(
                            item.ProductId,
                            item.Quantity))
            ]
        );

        var content = JsonSerializer.Serialize(integrationEvent);

        var message = new Message<string, string>
        {
            Key = order.Id.ToString(),
            Value = content,
        };

        var deliveryResult = await producer.ProduceAsync(
            options.Value.OrderCreatedTopic,
            message,
            cancellationToken
        );

        logger.LogInformation(
            "Published {EventType} for order {OrderId} to topic {Topic}, partition {Partition}, offset {Offset}",
            nameof(OrderCreatedIntegrationEvent),
            order.Id,
            deliveryResult.Topic,
            deliveryResult.Partition.Value,
            deliveryResult.Offset.Value
        );
    }
}
