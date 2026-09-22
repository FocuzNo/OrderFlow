using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Ordering.Application.Abstractions.Persistence;
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

        services.AddDbContext<OrderingDbContext>(options =>
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention()
        );
        services.AddScoped<IUnitOfWork>(provider =>
            provider.GetRequiredService<OrderingDbContext>()
        );
        services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }
}
