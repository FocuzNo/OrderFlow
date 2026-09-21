using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Infrastructure.Messaging;
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

        services
            .AddOptions<KafkaOptions>()
            .Bind(configuration.GetSection(KafkaOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.BootstrapServers),
                "Kafka:BootstrapServers is required."
            )
            .Validate(options => options.MaxRetries > 0, "Kafka:MaxRetries must be positive.")
            .Validate(
                options => options.OutboxBatchSize > 0,
                "Kafka:OutboxBatchSize must be positive."
            )
            .ValidateOnStart();

        services.AddDbContext<InventoryDbContext>(options =>
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention()
        );
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddSingleton<IKafkaPublisher, KafkaPublisher>();
        services.AddHostedService<OutboxProcessor>();
        services.AddHostedService<OrderSubmittedConsumer>();
        services.AddHealthChecks().AddCheck<KafkaHealthCheck>("kafka", tags: ["ready"]);

        return services;
    }
}
