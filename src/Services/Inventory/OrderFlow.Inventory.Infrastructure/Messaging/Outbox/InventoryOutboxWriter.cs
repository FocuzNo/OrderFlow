using System.Text.Json;
using Microsoft.Extensions.Options;
using OrderFlow.IntegrationEvents.Inventory;
using OrderFlow.Inventory.Application.Abstractions.Messaging;
using OrderFlow.Inventory.Application.Inventory;
using OrderFlow.Inventory.Infrastructure.Messaging.Kafka;
using OrderFlow.Inventory.Infrastructure.Persistence;
using OrderFlow.Inventory.Infrastructure.Persistence.Outbox;

namespace OrderFlow.Inventory.Infrastructure.Messaging.Outbox;

public sealed class InventoryOutboxWriter(
    InventoryDbContext dbContext,
    IOptions<KafkaOptions> kafkaOptions
) : IInventoryOutboxWriter
{
    public void AddReserved(
        Guid orderId,
        IReadOnlyCollection<ReservedStockItem> items
    )
    {
        var eventId =
            Guid.NewGuid();

        var occurredAt =
            DateTimeOffset.UtcNow;

        var integrationEvent =
            new InventoryReservedIntegrationEvent(
                eventId,
                occurredAt,
                orderId,
                items
                    .Select(item =>
                        new InventoryReservedItem(
                            item.ProductId,
                            item.StockItemId,
                            item.WarehouseId,
                            item.ReservationId,
                            item.Quantity
                        )
                    )
                    .ToArray()
            );

        var content =
            JsonSerializer.Serialize(
                integrationEvent
            );

        var outboxMessage =
            OutboxMessage.Create(
                eventId,
                kafkaOptions.Value.InventoryReservedTopic,
                nameof(InventoryReservedIntegrationEvent),
                orderId.ToString(),
                content,
                occurredAt
            );

        dbContext.OutboxMessages.Add(
            outboxMessage
        );
    }

    public void AddReservationFailed(
        Guid orderId,
        string reason
    )
    {
        var eventId =
            Guid.NewGuid();

        var occurredAt =
            DateTimeOffset.UtcNow;

        var integrationEvent =
            new InventoryReservationFailedIntegrationEvent(
                eventId,
                occurredAt,
                orderId,
                reason
            );

        var content =
            JsonSerializer.Serialize(
                integrationEvent
            );

        var outboxMessage =
            OutboxMessage.Create(
                eventId,
                kafkaOptions.Value.InventoryReservationFailedTopic,
                nameof(InventoryReservationFailedIntegrationEvent),
                orderId.ToString(),
                content,
                occurredAt
            );

        dbContext.OutboxMessages.Add(
            outboxMessage
        );
    }
}
