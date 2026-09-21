using System.Text.Json;
using Confluent.Kafka;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderFlow.IntegrationContracts;
using OrderFlow.Inventory.Application.Inventory;
using OrderFlow.Inventory.Infrastructure.Persistence;

namespace OrderFlow.Inventory.Infrastructure.Messaging;

public sealed class OrderSubmittedConsumer(
    IServiceScopeFactory scopes,
    IOptions<KafkaOptions> options,
    IKafkaPublisher publisher,
    ILogger<OrderSubmittedConsumer> logger
) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken cancellationToken) =>
        Task.Run(() => Consume(cancellationToken), cancellationToken);

    private async Task Consume(CancellationToken cancellationToken)
    {
        var consumerConfiguration = new ConsumerConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
            GroupId = options.Value.ConsumerGroup,
            EnableAutoCommit = false,
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };
        using var consumer = new ConsumerBuilder<string, string>(consumerConfiguration).Build();
        consumer.Subscribe(KafkaTopics.OrderEvents);
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ConsumeResult<string, string> result;
                try
                {
                    result = consumer.Consume(cancellationToken);
                }
                catch (ConsumeException exception)
                {
                    logger.LogError(exception, "Inventory Kafka consume error");
                    continue;
                }
                var handled = false;
                var max = Math.Max(1, options.Value.MaxRetries);
                for (var attempt = 1; attempt <= max && !handled; attempt++)
                {
                    try
                    {
                        handled = await Handle(result.Message.Value, cancellationToken);
                    }
                    catch (Exception exception) when (attempt < max)
                    {
                        logger.LogWarning(
                            exception,
                            "Inventory consumer retry {Attempt}/{MaxAttempts}",
                            attempt,
                            max
                        );
                        await Task.Delay(
                            TimeSpan.FromSeconds(Math.Min(attempt * 2, 10)),
                            cancellationToken
                        );
                    }
                    catch (Exception exception)
                    {
                        logger.LogError(
                            exception,
                            "Inventory message exhausted retries and moved to DLT"
                        );
                        await publisher.PublishAsync(
                            KafkaTopics.DeadLetters,
                            result.Message.Key,
                            result.Message.Value,
                            cancellationToken
                        );
                        handled = true;
                    }
                }
                if (handled)
                    consumer.Commit(result);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        finally
        {
            consumer.Close();
        }
    }

    private async Task<bool> Handle(string content, CancellationToken cancellationToken)
    {
        var envelope =
            JsonSerializer.Deserialize<IntegrationEventEnvelope>(content)
            ?? throw new JsonException("Envelope is invalid.");
        if (
            envelope.EventType != nameof(OrderSubmittedIntegrationEvent)
            && envelope.EventType != nameof(OrderCancelledIntegrationEvent)
        )
            return true;
        await using var scope = scopes.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        if (
            await databaseContext.InboxMessages.AnyAsync(
                candidate =>
                    candidate.Id == envelope.EventId
                    && candidate.Consumer == options.Value.ConsumerGroup,
                cancellationToken
            )
        )
            return true;
        await using var transaction = await databaseContext.Database.BeginTransactionAsync(
            cancellationToken
        );
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        if (envelope.EventType == nameof(OrderCancelledIntegrationEvent))
        {
            var cancelled = JsonSerializer.Deserialize<OrderCancelledIntegrationEvent>(
                envelope.Payload
            )!;
            await sender.Send(
                new InventoryFeatures.ReleaseOrderInventoryCommand(cancelled.OrderId),
                cancellationToken
            );
            databaseContext.InboxMessages.Add(
                new()
                {
                    Id = envelope.EventId,
                    Consumer = options.Value.ConsumerGroup,
                    ProcessedOnUtc = DateTimeOffset.UtcNow,
                }
            );
            await databaseContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        var message =
            JsonSerializer.Deserialize<OrderSubmittedIntegrationEvent>(envelope.Payload)
            ?? throw new JsonException("Order event is invalid.");
        var reservation = await sender.Send(
            new InventoryFeatures.ReserveOrderInventoryCommand(
                message.OrderId,
                message
                    .Items.Select(item => new InventoryFeatures.OrderInventoryItem(
                        item.ProductId,
                        item.Quantity
                    ))
                    .ToArray()
            ),
            cancellationToken
        );
        var outgoingEventId = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow;
        object outgoing = reservation.Succeeded
            ? new InventoryReservedIntegrationEvent(
                outgoingEventId,
                occurredAt,
                message.OrderId,
                reservation.ReservationIds
            )
            : new InventoryReservationFailedIntegrationEvent(
                outgoingEventId,
                occurredAt,
                message.OrderId,
                reservation.Error ?? "Inventory reservation failed."
            );
        var outgoingEnvelope = new IntegrationEventEnvelope(
            outgoingEventId,
            outgoing.GetType().Name,
            1,
            occurredAt,
            envelope.CorrelationId,
            envelope.EventId.ToString(),
            message.OrderId.ToString(),
            JsonSerializer.Serialize(outgoing, outgoing.GetType())
        );
        databaseContext.OutboxMessages.Add(
            new()
            {
                Id = outgoingEventId,
                Type = KafkaTopics.InventoryEvents,
                AggregateId = message.OrderId.ToString(),
                OccurredOnUtc = occurredAt,
                Content = JsonSerializer.Serialize(outgoingEnvelope),
            }
        );
        databaseContext.InboxMessages.Add(
            new()
            {
                Id = envelope.EventId,
                Consumer = options.Value.ConsumerGroup,
                ProcessedOnUtc = DateTimeOffset.UtcNow,
            }
        );
        await databaseContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }
}
