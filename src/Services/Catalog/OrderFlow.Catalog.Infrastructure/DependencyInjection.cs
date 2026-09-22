using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
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

        services.AddDbContext<CatalogDbContext>(options =>
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention()
        );
        services.AddScoped<IUnitOfWork>(provider =>
            provider.GetRequiredService<CatalogDbContext>()
        );
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        return services;
    }
}
