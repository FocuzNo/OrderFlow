using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrderFlow.Inventory.Application.Abstractions.Messaging;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Infrastructure.Messaging.Kafka;
using OrderFlow.Inventory.Infrastructure.Messaging.Outbox;
using OrderFlow.Inventory.Infrastructure.Persistence;
using OrderFlow.Inventory.Infrastructure.Persistence.Repositories;

namespace OrderFlow.Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString = configuration.GetConnectionString("InventoryDatabase")
            ?? throw new InvalidOperationException("ConnectionStrings:InventoryDatabase is required.");

        services.AddDbContext<InventoryDbContext>(options =>
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention()
        );
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<InventoryDbContext>());
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IInboxRepository, InboxRepository>();
        services.AddScoped<IInventoryOutboxWriter, InventoryOutboxWriter>();

        services.AddOptions<KafkaOptions>()
            .Bind(configuration.GetSection(KafkaOptions.SectionName))
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.BootstrapServers)
                && !string.IsNullOrWhiteSpace(options.ConsumerGroup)
                && !string.IsNullOrWhiteSpace(options.OrderCreatedTopic)
                && !string.IsNullOrWhiteSpace(options.OrderCreatedDeadLetterTopic)
                && !string.IsNullOrWhiteSpace(options.InventoryReservedTopic)
                && !string.IsNullOrWhiteSpace(options.InventoryReservationFailedTopic),
                "Kafka broker, consumer group and all topics are required."
            )
            .Validate(options =>
                options.ProcessingMaxRetries >= 0
                && options.RetryDelayMilliseconds > 0
                && options.MaxPollIntervalMilliseconds >= 300000,
                "Kafka retries must be nonnegative, delay positive and max poll interval at least 300000ms."
            )
            .ValidateOnStart();

        services.AddOptions<OutboxOptions>()
            .Bind(configuration.GetSection(OutboxOptions.SectionName))
            .Validate(options =>
                options.BatchSize > 0
                && options.PollingIntervalMilliseconds > 0
                && options.MaxRetries > 0,
                "Outbox batch size, polling interval and max retries must be positive."
            )
            .ValidateOnStart();

        services.AddSingleton<IProducer<string, string>>(provider =>
        {
            var kafkaOptions = provider.GetRequiredService<IOptions<KafkaOptions>>().Value;
            return new ProducerBuilder<string, string>(new ProducerConfig
            {
                BootstrapServers = kafkaOptions.BootstrapServers,
                ClientId = "orderflow-inventory",
                Acks = Acks.All,
                EnableIdempotence = true,
                MessageTimeoutMs = 5000,
            }).Build();
        });
        services.AddSingleton<IKafkaPublisher, KafkaPublisher>();
        services.AddHostedService<OrderCreatedKafkaConsumer>();
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}
