using System.Text.Json;
using Confluent.Kafka;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderFlow.IntegrationContracts;
using OrderFlow.Ordering.Application.Orders;
using OrderFlow.Ordering.Infrastructure.Persistence;

namespace OrderFlow.Ordering.Infrastructure.Messaging;

public sealed class WorkflowConsumer(
    IServiceScopeFactory scopes,
    IOptions<KafkaOptions> options,
    IKafkaPublisher publisher,
    ILogger<WorkflowConsumer> logger
) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken ct) => Task.Run(() => Consume(ct), ct);

    private async Task Consume(CancellationToken ct)
    {
        using var consumer = new ConsumerBuilder<string, string>(
            new ConsumerConfig
            {
                BootstrapServers = options.Value.BootstrapServers,
                GroupId = options.Value.ConsumerGroup,
                EnableAutoCommit = false,
                AutoOffsetReset = AutoOffsetReset.Earliest,
            }
        ).Build();
        consumer.Subscribe([KafkaTopics.InventoryEvents, KafkaTopics.PaymentEvents]);
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var result = consumer.Consume(ct);
                await HandleWithRetry(result, ct);
                consumer.Commit(result);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        finally
        {
            consumer.Close();
        }
    }

    private async Task HandleWithRetry(ConsumeResult<string, string> result, CancellationToken ct)
    {
        var attempts = Math.Max(1, options.Value.MaxRetries);
        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            try
            {
                await Handle(result.Message.Value, ct);
                return;
            }
            catch (Exception exception) when (attempt < attempts)
            {
                logger.LogWarning(
                    exception,
                    "Ordering workflow retry {Attempt}/{MaxAttempts}",
                    attempt,
                    attempts
                );
                await Task.Delay(TimeSpan.FromSeconds(Math.Min(attempt * 2, 10)), ct);
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Ordering workflow event exhausted retries and moved to DLT"
                );
                await publisher.PublishAsync(
                    KafkaTopics.DeadLetters,
                    result.Message.Key,
                    result.Message.Value,
                    ct
                );
            }
        }
    }

    private async Task Handle(string content, CancellationToken ct)
    {
        var envelope =
            JsonSerializer.Deserialize<IntegrationEventEnvelope>(content)
            ?? throw new JsonException("Envelope is invalid.");
        await using var scope = scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        if (
            await db.InboxMessages.AnyAsync(
                x => x.Id == envelope.EventId && x.Consumer == options.Value.ConsumerGroup,
                ct
            )
        )
            return;
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        switch (envelope.EventType)
        {
            case nameof(InventoryReservedIntegrationEvent):
                await sender.Send(
                    new OrderFeatures.InventoryReserved(
                        JsonSerializer
                            .Deserialize<InventoryReservedIntegrationEvent>(envelope.Payload)!
                            .OrderId
                    ),
                    ct
                );
                break;
            case nameof(InventoryReservationFailedIntegrationEvent):
                var inventoryFailed =
                    JsonSerializer.Deserialize<InventoryReservationFailedIntegrationEvent>(
                        envelope.Payload
                    )!;
                await sender.Send(
                    new OrderFeatures.WorkflowFailed(
                        inventoryFailed.OrderId,
                        inventoryFailed.Reason
                    ),
                    ct
                );
                break;
            case nameof(PaymentSucceededIntegrationEvent):
                await sender.Send(
                    new OrderFeatures.PaymentSucceeded(
                        JsonSerializer
                            .Deserialize<PaymentSucceededIntegrationEvent>(envelope.Payload)!
                            .OrderId
                    ),
                    ct
                );
                break;
            case nameof(PaymentFailedIntegrationEvent):
                var paymentFailed = JsonSerializer.Deserialize<PaymentFailedIntegrationEvent>(
                    envelope.Payload
                )!;
                await sender.Send(
                    new OrderFeatures.WorkflowFailed(paymentFailed.OrderId, paymentFailed.Reason),
                    ct
                );
                break;
            default:
                return;
        }
        db.InboxMessages.Add(
            new()
            {
                Id = envelope.EventId,
                Consumer = options.Value.ConsumerGroup,
                ProcessedOnUtc = DateTimeOffset.UtcNow,
            }
        );
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }
}
