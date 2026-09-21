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
    public static IServiceCollection AddInfrastructure(this IServiceCollection s, IConfiguration c)
    {
        var cs =
            c.GetConnectionString("PaymentsDatabase")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:PaymentsDatabase is required."
            );
        _ =
            c[$"{KafkaOptions.SectionName}:BootstrapServers"]
            ?? throw new InvalidOperationException("Kafka:BootstrapServers is required.");
        s.Configure<KafkaOptions>(c.GetSection(KafkaOptions.SectionName));
        s.Configure<DevelopmentPaymentGatewayOptions>(
            c.GetSection(DevelopmentPaymentGatewayOptions.SectionName)
        );
        s.AddDbContext<PaymentsDbContext>(x => x.UseNpgsql(cs).UseSnakeCaseNamingConvention());
        s.AddScoped<IPaymentRepository, PaymentRepository>();
        s.AddSingleton<IPaymentGateway, DevelopmentPaymentGateway>();
        s.AddSingleton<IKafkaPublisher, KafkaPublisher>();
        s.AddHostedService<OutboxProcessor>();
        s.AddHostedService<PaymentRequestedConsumer>();
        s.AddHealthChecks().AddCheck<KafkaHealthCheck>("kafka", tags: ["ready"]);
        return s;
    }
}
