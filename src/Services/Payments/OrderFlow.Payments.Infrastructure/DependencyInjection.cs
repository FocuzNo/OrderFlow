using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Payments.Application.Abstractions.Payments;
using OrderFlow.Payments.Application.Abstractions.Persistence;
using OrderFlow.Payments.Infrastructure.Messaging;
using OrderFlow.Payments.Infrastructure.Payments;
using OrderFlow.Payments.Infrastructure.Persistence;

namespace OrderFlow.Payments.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString =
            configuration.GetConnectionString("PaymentsDatabase")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:PaymentsDatabase is required."
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

        services.Configure<DevelopmentPaymentGatewayOptions>(
            configuration.GetSection(DevelopmentPaymentGatewayOptions.SectionName)
        );
        services.AddDbContext<PaymentsDbContext>(options =>
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention()
        );
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddSingleton<IPaymentGateway, DevelopmentPaymentGateway>();
        services.AddSingleton<IKafkaPublisher, KafkaPublisher>();
        services.AddHostedService<OutboxProcessor>();
        services.AddHostedService<PaymentRequestedConsumer>();
        services.AddHealthChecks().AddCheck<KafkaHealthCheck>("kafka", tags: ["ready"]);

        return services;
    }
}
