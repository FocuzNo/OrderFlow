using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Infrastructure.Messaging;
using OrderFlow.Inventory.Infrastructure.Persistence;

namespace OrderFlow.Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection s, IConfiguration c)
    {
        var cs =
            c.GetConnectionString("InventoryDatabase")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:InventoryDatabase is required."
            );
        _ =
            c[$"{KafkaOptions.SectionName}:BootstrapServers"]
            ?? throw new InvalidOperationException("Kafka:BootstrapServers is required.");
        s.Configure<KafkaOptions>(c.GetSection(KafkaOptions.SectionName));
        s.AddDbContext<InventoryDbContext>(x => x.UseNpgsql(cs).UseSnakeCaseNamingConvention());
        s.AddScoped<IInventoryRepository, InventoryRepository>();
        s.AddSingleton<IKafkaPublisher, KafkaPublisher>();
        s.AddHostedService<OutboxProcessor>();
        s.AddHostedService<OrderSubmittedConsumer>();
        s.AddHealthChecks().AddCheck<KafkaHealthCheck>("kafka", tags: ["ready"]);
        return s;
    }
}
