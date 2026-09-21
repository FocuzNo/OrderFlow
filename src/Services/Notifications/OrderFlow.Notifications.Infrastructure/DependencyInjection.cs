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
    public static IServiceCollection AddInfrastructure(this IServiceCollection s, IConfiguration c)
    {
        var cs =
            c.GetConnectionString("NotificationsDatabase")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:NotificationsDatabase is required."
            );
        _ =
            c[$"{KafkaOptions.SectionName}:BootstrapServers"]
            ?? throw new InvalidOperationException("Kafka:BootstrapServers is required.");
        s.Configure<KafkaOptions>(c.GetSection(KafkaOptions.SectionName));
        s.AddDbContext<NotificationsDbContext>(x => x.UseNpgsql(cs).UseSnakeCaseNamingConvention());
        s.AddScoped<INotificationRepository, NotificationRepository>();
        s.AddSingleton<IEmailSender, LoggingEmailSender>();
        s.AddSingleton<IKafkaPublisher, KafkaPublisher>();
        s.AddHostedService<OutboxProcessor>();
        s.AddHostedService<OrderOutcomeConsumer>();
        s.AddHealthChecks().AddCheck<KafkaHealthCheck>("kafka", tags: ["ready"]);
        return s;
    }
}
