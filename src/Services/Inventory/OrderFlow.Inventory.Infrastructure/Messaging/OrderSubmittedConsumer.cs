using System.Text.Json;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderFlow.IntegrationContracts;
using OrderFlow.Inventory.Infrastructure.Persistence;

namespace OrderFlow.Inventory.Infrastructure.Messaging;

public sealed class OrderSubmittedConsumer(
    IServiceScopeFactory scopes,
    IOptions<KafkaOptions> options,
    IKafkaPublisher publisher,
    ILogger<OrderSubmittedConsumer> logger
) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken ct) => Task.Run(() => Consume(ct), ct);

    private async Task Consume(CancellationToken ct)
    {
        var cfg = new ConsumerConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
            GroupId = options.Value.ConsumerGroup,
            EnableAutoCommit = false,
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };
        using var consumer = new ConsumerBuilder<string, string>(cfg).Build();
        consumer.Subscribe(KafkaTopics.OrderEvents);
        try
        {
            while (!ct.IsCancellationRequested)
            {
                ConsumeResult<string, string> result;
                try
                {
                    result = consumer.Consume(ct);
                }
                catch (ConsumeException ex)
                {
                    logger.LogError(ex, "Inventory Kafka consume error");
                    continue;
                }
                var handled = false;
                var max = Math.Max(1, options.Value.MaxRetries);
                for (var attempt = 1; attempt <= max && !handled; attempt++)
                {
                    try
                    {
                        handled = await Handle(result.Message.Value, ct);
                    }
                    catch (Exception ex) when (attempt < max)
                    {
                        logger.LogWarning(
                            ex,
                            "Inventory consumer retry {Attempt}/{MaxAttempts}",
                            attempt,
                            max
                        );
                        await Task.Delay(TimeSpan.FromSeconds(Math.Min(attempt * 2, 10)), ct);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Inventory message exhausted retries and moved to DLT");
                        await publisher.PublishAsync(
                            KafkaTopics.DeadLetters,
                            result.Message.Key,
                            result.Message.Value,
                            ct
                        );
                        handled = true;
                    }
                }
                if (handled)
                    consumer.Commit(result);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        finally
        {
            consumer.Close();
        }
    }

    private async Task<bool> Handle(string content, CancellationToken ct)
    {
        var env =
            JsonSerializer.Deserialize<IntegrationEventEnvelope>(content)
            ?? throw new JsonException("Envelope is invalid.");
        if (
            env.EventType != nameof(OrderSubmittedIntegrationEvent)
            && env.EventType != nameof(OrderCancelledIntegrationEvent)
        )
            return true;
        await using var scope = scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        if (
            await db.InboxMessages.AnyAsync(
                x => x.Id == env.EventId && x.Consumer == options.Value.ConsumerGroup,
                ct
            )
        )
            return true;
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        if (env.EventType == nameof(OrderCancelledIntegrationEvent))
        {
            var cancelled = JsonSerializer.Deserialize<OrderCancelledIntegrationEvent>(
                env.Payload
            )!;
            var stocks = await db
                .StockItems.Include(x => x.Reservations)
                .Where(x =>
                    x.Reservations.Any(r =>
                        r.OrderId == cancelled.OrderId
                        && r.Status
                            == OrderFlow.Inventory.Domain.Reservations.ReservationStatus.Pending
                    )
                )
                .ToListAsync(ct);
            foreach (var stock in stocks)
            {
                foreach (
                    var reservation in stock
                        .Reservations.Where(r =>
                            r.OrderId == cancelled.OrderId
                            && r.Status
                                == OrderFlow.Inventory.Domain.Reservations.ReservationStatus.Pending
                        )
                        .ToArray()
                )
                    stock.Release(reservation.Id);
            }
            db.InboxMessages.Add(
                new()
                {
                    Id = env.EventId,
                    Consumer = options.Value.ConsumerGroup,
                    ProcessedOnUtc = DateTimeOffset.UtcNow,
                }
            );
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return true;
        }
        var message =
            JsonSerializer.Deserialize<OrderSubmittedIntegrationEvent>(env.Payload)
            ?? throw new JsonException("Order event is invalid.");
        var ids = new List<Guid>();
        string? error = null;
        foreach (var item in message.Items)
        {
            var stock = await db
                .StockItems.Include(x => x.Reservations)
                .Where(x => x.ProductId == item.ProductId)
                .OrderByDescending(x => x.QuantityOnHand - x.ReservedQuantity)
                .FirstOrDefaultAsync(ct);
            if (stock is null || stock.AvailableQuantity < item.Quantity)
            {
                error = $"Insufficient stock for product {item.ProductId}.";
                break;
            }
            ids.Add(stock.Reserve(message.OrderId, item.Quantity).Id);
        }
        object outgoing = error is null
            ? new InventoryReservedIntegrationEvent(message.OrderId, ids)
            : new InventoryReservationFailedIntegrationEvent(message.OrderId, error);
        var outId = Guid.NewGuid();
        var outEnv = new IntegrationEventEnvelope(
            outId,
            outgoing.GetType().Name,
            1,
            DateTimeOffset.UtcNow,
            env.CorrelationId,
            env.EventId.ToString(),
            message.OrderId.ToString(),
            JsonSerializer.Serialize(outgoing, outgoing.GetType())
        );
        db.OutboxMessages.Add(
            new()
            {
                Id = outId,
                Type = KafkaTopics.InventoryEvents,
                AggregateId = message.OrderId.ToString(),
                OccurredOnUtc = DateTimeOffset.UtcNow,
                Content = JsonSerializer.Serialize(outEnv),
            }
        );
        db.InboxMessages.Add(
            new()
            {
                Id = env.EventId,
                Consumer = options.Value.ConsumerGroup,
                ProcessedOnUtc = DateTimeOffset.UtcNow,
            }
        );
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return true;
    }
}
