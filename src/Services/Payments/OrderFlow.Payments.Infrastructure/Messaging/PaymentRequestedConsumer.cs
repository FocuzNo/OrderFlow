using System.Text.Json;
using Confluent.Kafka;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderFlow.IntegrationContracts;
using OrderFlow.Payments.Application.Payments;
using OrderFlow.Payments.Infrastructure.Persistence;

namespace OrderFlow.Payments.Infrastructure.Messaging;

public sealed class PaymentRequestedConsumer(
    IServiceScopeFactory scopes,
    IOptions<KafkaOptions> options,
    IKafkaPublisher publisher,
    ILogger<PaymentRequestedConsumer> logger
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
        consumer.Subscribe(KafkaTopics.OrderEvents);
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var consumeResult = consumer.Consume(cancellationToken);
                await Retry(consumeResult, cancellationToken);
                consumer.Commit(consumeResult);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        finally
        {
            consumer.Close();
        }
    }

    private async Task Retry(
        ConsumeResult<string, string> consumeResult,
        CancellationToken cancellationToken
    )
    {
        var max = Math.Max(1, options.Value.MaxRetries);
        for (var attempt = 1; attempt <= max; attempt++)
        {
            try
            {
                await Handle(consumeResult.Message.Value, cancellationToken);
                return;
            }
            catch (Exception exception) when (attempt < max)
            {
                logger.LogWarning(
                    exception,
                    "Payment consumer retry {Attempt}/{MaxAttempts}",
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
                logger.LogError(exception, "Payment event exhausted retries and moved to DLT");
                await publisher.PublishAsync(
                    KafkaTopics.DeadLetters,
                    consumeResult.Message.Key,
                    consumeResult.Message.Value,
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
        if (envelope.EventType != nameof(PaymentRequestedIntegrationEvent))
            return;
        await using var scope = scopes.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
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
        var integrationEvent =
            JsonSerializer.Deserialize<PaymentRequestedIntegrationEvent>(envelope.Payload)
            ?? throw new JsonException("Payment event is invalid.");
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var payment = await sender.Send(
            new PaymentFeatures.CreatePaymentCommand(
                integrationEvent.OrderId,
                integrationEvent.Amount,
                "Card"
            ),
            cancellationToken
        );
        await sender.Send(new PaymentFeatures.ProcessPaymentCommand(payment.Id), cancellationToken);
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
