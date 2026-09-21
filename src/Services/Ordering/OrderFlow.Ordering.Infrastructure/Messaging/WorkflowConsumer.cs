using System.Text.Json;
using Confluent.Kafka;
using MediatR;
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
    protected override Task ExecuteAsync(CancellationToken cancellationToken) =>
        Task.Run(() => Consume(cancellationToken), cancellationToken);

    private async Task Consume(CancellationToken cancellationToken)
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
            while (!cancellationToken.IsCancellationRequested)
            {
                var result = consumer.Consume(cancellationToken);
                await HandleWithRetry(result, cancellationToken);
                consumer.Commit(result);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        finally
        {
            consumer.Close();
        }
    }

    private async Task HandleWithRetry(
        ConsumeResult<string, string> result,
        CancellationToken cancellationToken
    )
    {
        var attempts = Math.Max(1, options.Value.MaxRetries);
        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            try
            {
                await Handle(result.Message.Value, cancellationToken);
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
                await Task.Delay(
                    TimeSpan.FromSeconds(Math.Min(attempt * 2, 10)),
                    cancellationToken
                );
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
                    cancellationToken
                );
            }
        }
    }

    private async Task Handle(string content, CancellationToken cancellationToken)
    {
        var envelope =
            JsonSerializer.Deserialize<IntegrationEventEnvelope>(content)
            ?? throw new JsonException("Envelope is invalid.");
        await using var scope = scopes.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        if (
            await databaseContext.InboxMessages.AnyAsync(
                candidate =>
                    candidate.Id == envelope.EventId
                    && candidate.Consumer == options.Value.ConsumerGroup,
                cancellationToken
            )
        )
            return;
        await using var transaction = await databaseContext.Database.BeginTransactionAsync(
            cancellationToken
        );
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        switch (envelope.EventType)
        {
            case nameof(InventoryReservedIntegrationEvent):
                await sender.Send(
                    new OrderFeatures.MarkInventoryReservedCommand(
                        JsonSerializer
                            .Deserialize<InventoryReservedIntegrationEvent>(envelope.Payload)!
                            .OrderId
                    ),
                    cancellationToken
                );
                break;
            case nameof(InventoryReservationFailedIntegrationEvent):
                var inventoryFailed =
                    JsonSerializer.Deserialize<InventoryReservationFailedIntegrationEvent>(
                        envelope.Payload
                    )!;
                await sender.Send(
                    new OrderFeatures.FailOrderWorkflowCommand(
                        inventoryFailed.OrderId,
                        inventoryFailed.Reason
                    ),
                    cancellationToken
                );
                break;
            case nameof(PaymentSucceededIntegrationEvent):
                await sender.Send(
                    new OrderFeatures.ConfirmOrderPaymentCommand(
                        JsonSerializer
                            .Deserialize<PaymentSucceededIntegrationEvent>(envelope.Payload)!
                            .OrderId
                    ),
                    cancellationToken
                );
                break;
            case nameof(PaymentFailedIntegrationEvent):
                var paymentFailed = JsonSerializer.Deserialize<PaymentFailedIntegrationEvent>(
                    envelope.Payload
                )!;
                await sender.Send(
                    new OrderFeatures.FailOrderWorkflowCommand(
                        paymentFailed.OrderId,
                        paymentFailed.Reason
                    ),
                    cancellationToken
                );
                break;
            default:
                return;
        }
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
    }
}
