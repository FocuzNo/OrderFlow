using System.Text.Json;
using Confluent.Kafka;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
    protected override Task ExecuteAsync(CancellationToken ct) => Task.Run(() => Consume(ct), ct);

    private async Task Consume(CancellationToken ct)
    {
        using var c = new ConsumerBuilder<string, string>(
            new ConsumerConfig
            {
                BootstrapServers = options.Value.BootstrapServers,
                GroupId = options.Value.ConsumerGroup,
                EnableAutoCommit = false,
                AutoOffsetReset = AutoOffsetReset.Earliest,
            }
        ).Build();
        c.Subscribe(KafkaTopics.OrderEvents);
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var r = c.Consume(ct);
                await Retry(r, ct);
                c.Commit(r);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        finally
        {
            c.Close();
        }
    }

    private async Task Retry(ConsumeResult<string, string> r, CancellationToken ct)
    {
        var max = Math.Max(1, options.Value.MaxRetries);
        for (var attempt = 1; attempt <= max; attempt++)
        {
            try
            {
                await Handle(r.Message.Value, ct);
                return;
            }
            catch (Exception ex) when (attempt < max)
            {
                logger.LogWarning(
                    ex,
                    "Payment consumer retry {Attempt}/{MaxAttempts}",
                    attempt,
                    max
                );
                await Task.Delay(TimeSpan.FromSeconds(Math.Min(attempt * 2, 10)), ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Payment event exhausted retries and moved to DLT");
                await publisher.PublishAsync(
                    KafkaTopics.DeadLetters,
                    r.Message.Key,
                    r.Message.Value,
                    ct
                );
            }
        }
    }

    private async Task Handle(string content, CancellationToken ct)
    {
        var env =
            JsonSerializer.Deserialize<IntegrationEventEnvelope>(content)
            ?? throw new JsonException("Envelope is invalid.");
        if (env.EventType != nameof(PaymentRequestedIntegrationEvent))
            return;
        await using var scope = scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
        if (
            await db.InboxMessages.AnyAsync(
                x => x.Id == env.EventId && x.Consumer == options.Value.ConsumerGroup,
                ct
            )
        )
            return;
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var e =
            JsonSerializer.Deserialize<PaymentRequestedIntegrationEvent>(env.Payload)
            ?? throw new JsonException("Payment event is invalid.");
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var payment = await sender.Send(
            new PaymentFeatures.Create(e.OrderId, e.Amount, "Card"),
            ct
        );
        await sender.Send(new PaymentFeatures.Process(payment.Id), ct);
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
    }
}
