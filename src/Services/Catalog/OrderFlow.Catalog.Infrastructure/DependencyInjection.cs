using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Infrastructure.Messaging;
using OrderFlow.Catalog.Infrastructure.Persistence;
using OrderFlow.Catalog.Infrastructure.Persistence.Repositories;

namespace OrderFlow.Catalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString =
            configuration.GetConnectionString("CatalogDatabase")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:CatalogDatabase is required."
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

        services.AddDbContext<CatalogDbContext>(options =>
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention()
        );
        services.AddScoped<IUnitOfWork>(provider =>
            provider.GetRequiredService<CatalogDbContext>()
        );
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddSingleton<IKafkaPublisher, KafkaPublisher>();
        services.AddHostedService<OutboxProcessor>();
        services.AddHealthChecks().AddCheck<KafkaHealthCheck>("kafka", tags: ["ready"]);

        return services;
    }
}
