using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrderFlow.Ordering.Application.Abstractions.Messaging;
using OrderFlow.Ordering.Application.Abstractions.Persistence;
using OrderFlow.Ordering.Infrastructure.Messaging.Kafka;
using OrderFlow.Ordering.Infrastructure.Messaging.Kafka.Producers;
using OrderFlow.Ordering.Infrastructure.Persistence;

namespace OrderFlow.Ordering.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString =
            configuration.GetConnectionString("OrderingDatabase")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:OrderingDatabase is required."
            );

        services.AddDbContext<OrderingDbContext>(options =>
            options
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention()
        );

        services.AddScoped<IUnitOfWork>(provider =>
            provider.GetRequiredService<OrderingDbContext>()
        );

        services.AddScoped<IOrderRepository, OrderRepository>();

        services
            .AddOptions<KafkaOptions>()
            .Bind(configuration.GetSection(KafkaOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.BootstrapServers),
                "Kafka:BootstrapServers is required."
            )
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.OrderCreatedTopic),
                "Kafka:OrderCreatedTopic is required."
            )
            .ValidateOnStart();

        services.AddSingleton<IProducer<string, string>>(serviceProvider =>
        {
            var kafkaOptions = serviceProvider
                .GetRequiredService<IOptions<KafkaOptions>>()
                .Value;

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = kafkaOptions.BootstrapServers,
                ClientId = "orderflow-ordering",
                Acks = Acks.All,
                EnableIdempotence = true,
            };

            return new ProducerBuilder<string, string>(
                producerConfig
            ).Build();
        });

        services.AddSingleton<
            IOrderCreatedPublisher,
            OrderCreatedKafkaPublisher
        >();

        return services;
    }
}
