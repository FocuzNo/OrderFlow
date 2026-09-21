using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Notifications.Application.Abstractions.Delivery;
using OrderFlow.Notifications.Application.Abstractions.Persistence;
using OrderFlow.Notifications.Infrastructure.Delivery;
using OrderFlow.Notifications.Infrastructure.Messaging;
using OrderFlow.Notifications.Infrastructure.Persistence;

namespace OrderFlow.Notifications.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString =
            configuration.GetConnectionString("NotificationsDatabase")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:NotificationsDatabase is required."
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

        services.AddDbContext<NotificationsDbContext>(options =>
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention()
        );
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddSingleton<IEmailSender, LoggingEmailSender>();
        services.AddSingleton<IKafkaPublisher, KafkaPublisher>();
        services.AddHostedService<OutboxProcessor>();
        services.AddHostedService<OrderOutcomeConsumer>();
        services.AddHealthChecks().AddCheck<KafkaHealthCheck>("kafka", tags: ["ready"]);

        return services;
    }
}
