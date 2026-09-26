using System.Text.Json;
using Microsoft.Extensions.Options;
using OrderFlow.IntegrationEvents.Orders;
using OrderFlow.Ordering.Application.Abstractions.Messaging;
using OrderFlow.Ordering.Domain.Orders;
using OrderFlow.Ordering.Infrastructure.Messaging.Kafka;
using OrderFlow.Ordering.Infrastructure.Persistence;
using OrderFlow.Ordering.Infrastructure.Persistence.Outbox;

namespace OrderFlow.Ordering.Infrastructure.Messaging.Outbox;

public sealed class OrderCreatedOutboxWriter(
    OrderingDbContext dbContext,
    IOptions<KafkaOptions> kafkaOptions
) : IOrderCreatedOutboxWriter
{
    public void Add(
        Order order
    )
    {
        var eventId = Guid.NewGuid();

        var integrationEvent =
            new OrderCreatedIntegrationEvent(
                eventId,
                order.CreatedAt,
                order.Id,
                order.Items
                    .Select(item =>
                        new OrderCreatedItem(
                            item.ProductId,
                            item.Quantity
                        )
                    )
                    .ToArray()
            );

        var content = JsonSerializer.Serialize(
            integrationEvent
        );

        var outboxMessage = OutboxMessage.Create(
            eventId,
            kafkaOptions.Value.OrderCreatedTopic,
            nameof(OrderCreatedIntegrationEvent),
            order.Id.ToString(),
            content,
            order.CreatedAt
        );

        dbContext.OutboxMessages.Add(
            outboxMessage
        );
    }
}
