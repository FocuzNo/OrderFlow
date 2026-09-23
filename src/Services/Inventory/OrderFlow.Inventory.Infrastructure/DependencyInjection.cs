using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
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

        services.AddDbContext<InventoryDbContext>(options =>
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention()
        );
        services.AddScoped<IUnitOfWork>(provider =>
            provider.GetRequiredService<InventoryDbContext>()
        );
        services.AddScoped<IInventoryRepository, InventoryRepository>();

        return services;
    }
}
