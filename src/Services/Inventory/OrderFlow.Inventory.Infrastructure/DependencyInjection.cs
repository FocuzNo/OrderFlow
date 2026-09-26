using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Infrastructure.Messaging.Kafka;
using OrderFlow.Inventory.Infrastructure.Persistence;

namespace OrderFlow.Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString =
            configuration.GetConnectionString("InventoryDatabase")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:InventoryDatabase is required."
            );

        services.AddDbContext<InventoryDbContext>(options =>
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention()
        );
        services.AddScoped<IUnitOfWork>(provider =>
            provider.GetRequiredService<InventoryDbContext>()
        );
        services.AddScoped<IInventoryRepository, InventoryRepository>();

        services
            .AddOptions<KafkaOptions>()
            .Bind(
                configuration.GetSection(
                    KafkaOptions.SectionName
                )
            )
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.BootstrapServers
                    ),
                "Kafka:BootstrapServers is required."
            )
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.OrderCreatedTopic
                    ),
                "Kafka:OrderCreatedTopic is required."
            )
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.ConsumerGroup
                    ),
                "Kafka:ConsumerGroup is required."
            )
            .ValidateOnStart();

        services.AddHostedService<
            OrderCreatedKafkaConsumer
        >();

        return services;
    }
}
