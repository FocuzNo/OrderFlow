using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Infrastructure.Messaging;
using OrderFlow.Catalog.Infrastructure.Persistence;
using OrderFlow.Catalog.Infrastructure.Persistence.Repositories;

namespace OrderFlow.Catalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection s, IConfiguration c)
    {
        var cs =
            c.GetConnectionString("CatalogDatabase")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:CatalogDatabase is required."
            );
        _ =
            c[$"{KafkaOptions.SectionName}:BootstrapServers"]
            ?? throw new InvalidOperationException("Kafka:BootstrapServers is required.");
        s.Configure<KafkaOptions>(c.GetSection(KafkaOptions.SectionName));
        s.AddDbContext<CatalogDbContext>(x => x.UseNpgsql(cs).UseSnakeCaseNamingConvention());
        s.AddScoped<IProductRepository, ProductRepository>();
        s.AddScoped<ICategoryRepository, CategoryRepository>();
        s.AddSingleton<IKafkaPublisher, KafkaPublisher>();
        s.AddHostedService<OutboxProcessor>();
        s.AddHealthChecks().AddCheck<KafkaHealthCheck>("kafka", tags: ["ready"]);
        return s;
    }
}
