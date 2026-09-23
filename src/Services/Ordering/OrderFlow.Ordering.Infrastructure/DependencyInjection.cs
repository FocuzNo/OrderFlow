using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Ordering.Application.Abstractions.Persistence;
using OrderFlow.Ordering.Infrastructure.Messaging;
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

        services.AddDbContext<OrderingDbContext>(options =>
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention()
        );
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddSingleton<IKafkaPublisher, KafkaPublisher>();
        // services.AddHostedService<OutboxProcessor>();
        // services.AddHostedService<WorkflowConsumer>();
        services.AddHealthChecks().AddCheck<KafkaHealthCheck>("kafka", tags: ["ready"]);

        return services;
    }
}
